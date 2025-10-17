using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngrySharkStudio.LLM.API;
using AngrySharkStudio.LLM.Core;
using AngrySharkStudio.LLM.Models.Conversation;
using AngrySharkStudio.LLM.Models.Debug;
using AngrySharkStudio.LLM.PlatformDebuggers;
using AngrySharkStudio.LLM.ScriptableObjects;
using UnityEngine;
using UnityEngine.Events;

namespace AngrySharkStudio.LLM.Examples {
    public class SmartNpcExample : MonoBehaviour {

        [Header("NPC Configuration")]
        [Tooltip("The character profile ScriptableObject containing all NPC configuration")]
        [SerializeField] private CharacterProfile characterProfile;


        [Header("UI Events")]
        [Space(5)]
        public UnityEvent<string> onResponseReceived = new();
        public UnityEvent<string> onResponseError = new();
        public UnityEvent<bool> onProcessingStateChanged = new();

        [Header("AI Components")]
        [SerializeField] private AiResponseDebugger debugger;
        [SerializeField] private NpcCharacterConsistency characterConsistency;
        [SerializeField] private AiContentFilter contentFilter;
        [SerializeField] private AiResponseCache responseCache;
        // Can be ChatGptDebugger, ClaudeDebugger, or GeminiDebugger
        [SerializeField] private AiDebuggerBase platformDebugger;

        [Header("Runtime References")]
        private LlmManager llmManager;

        [Header("Conversation State")]
        private readonly List<ConversationTurn> conversationHistory = new();

        public bool IsProcessing { get; private set; }

        private void OnValidate() {
            // Auto-assign components if not set in inspector
            if (debugger == null) {
                debugger = GetComponent<AiResponseDebugger>();
            }

            if (characterConsistency == null) {
                characterConsistency = GetComponent<NpcCharacterConsistency>();
            }

            if (contentFilter == null) {
                contentFilter = GetComponent<AiContentFilter>();
            }

            if (responseCache == null) {
                responseCache = GetComponent<AiResponseCache>();
            }

            // Find any platform debugger (base class)
            if (platformDebugger == null) {
                platformDebugger = GetComponent<AiDebuggerBase>();

                if (platformDebugger == null) {
                    // Try to find any derived debugger
                    platformDebugger = GetComponent<ChatGptDebugger>() ??
                                       GetComponent<ClaudeDebugger>() as AiDebuggerBase ??
                                       GetComponent<GeminiDebugger>();
                }
            }
        }

        private void Awake() {
            // Validate character profile
            if (characterProfile == null) {
                Debug.LogError($"No CharacterProfile assigned to {gameObject.name}!");

                return;
            }

            // Initialize components with character profile
            InitializeComponents();
        }

        private void Start() {
            // Ensure LlmManager exists (do this in Start for proper singleton access)
            if (llmManager == null) {
                llmManager = LlmManager.Instance;

                if (llmManager == null) {
                    Debug.LogError(
                        $"[{characterProfile.NpcName}] LlmManager not found! Please ensure LlmManager exists in the scene.");
                }
            }

            // Preload common responses for consistency
            if (responseCache != null && characterProfile != null) {
                responseCache.PreloadCommonResponses(characterProfile.NpcName);
            }
        }

        private void InitializeComponents() {
            // Initialize character consistency with the profile
            if (characterConsistency != null) {
                characterConsistency.SetCharacterProfile(characterProfile);
            }

            // Initialize the debugger with character settings
            if (debugger != null) {
                debugger.SetCharacterName(characterProfile.NpcName);
                debugger.SetMaxResponseLength(characterProfile.MaxResponseLength);
                debugger.SetBannedPhrases(new List<string>(characterProfile.BannedPhrases));
            }
        }

        public async Task ProcessPlayerInput(string playerInput) {
            if (IsProcessing) {
                Debug.LogWarning($"[{characterProfile.NpcName}] Still processing previous input!");
                onResponseError?.Invoke("Please wait for the previous response.");

                return;
            }

            Debug.Log($"[{characterProfile.NpcName}] Player says: \"{playerInput}\"");

            IsProcessing = true;
            onProcessingStateChanged?.Invoke(true);

            var responseStartTime = Time.realtimeSinceStartup;

            try {
                // Step 1: Check cache first
                var cachedResponse = responseCache.GetCachedResponse(
                    playerInput,
                    characterProfile.NpcName,
                    characterConsistency.CurrentMood
                );

                if (!string.IsNullOrEmpty(cachedResponse)) {
                    DisplayResponse(cachedResponse);

                    return;
                }

                // Step 2: Build a character-consistent prompt
                var aiPrompt = characterConsistency.BuildCharacterPrompt(
                    playerInput,
                    conversationHistory
                );

                // Step 3: Get AI response through LLMManager
                var apiStartTime = Time.realtimeSinceStartup;
                // Direct API call - suitable for single NPCs or low-traffic scenarios
                var rawResponse = await llmManager.GetAIResponse(aiPrompt);

                // For multiple NPCs or high-traffic scenarios, use AiRequestQueue:
                // var queue = GetComponent<AiRequestQueue>();
                // string rawResponse = await queue.QueueRequest(characterProfile.NpcName, aiPrompt, priority: 0);

                var apiResponseTime = Time.realtimeSinceStartup - apiStartTime;

                // Step 4: Validate response
                var validationResult = debugger.ValidateResponse(rawResponse, aiPrompt);

                string finalResponse;

                if (validationResult.passed) {
                    // Step 5: Apply content filter
                    var filterResult = contentFilter.FilterContent(rawResponse);

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (filterResult.passed) {
                        finalResponse = filterResult.filteredContent;
                    } else {
                        // Use fallback if the content filter fails
                        finalResponse = AiContentFilter.GetSafeFallbackResponse();
                    }
                } else {
                    // Use fallback if validation fails
                    finalResponse = GetFallbackResponse(validationResult);
                }

                // Step 6: Final era consistency check
                if (!characterConsistency.ValidateEraConsistency(finalResponse)) {
                    finalResponse = "I don't understand what you're talking about.";
                }

                // Step 7: Cache the successful response
                responseCache.CacheResponse(
                    playerInput,
                    finalResponse,
                    characterProfile.NpcName,
                    characterConsistency.CurrentMood
                );

                // Step 8: Log to debugger
                var totalTime = Time.realtimeSinceStartup - responseStartTime;

                debugger.LogDebugEntry(
                    playerInput,
                    aiPrompt,
                    rawResponse,
                    finalResponse,
                    validationResult,
                    totalTime,
                    "ChatGPT" // Or current platform
                );

                // Step 9: Update conversation history
                conversationHistory.Add(new ConversationTurn {
                    playerInput = playerInput,
                    npcResponse = finalResponse,
                    timestamp = Time.time
                });

                // Keep history size manageable
                if (conversationHistory.Count > 10) {
                    conversationHistory.RemoveAt(0);
                }

                // Step 10: Display response
                DisplayResponse(finalResponse);
            } catch (Exception e) {
                Debug.LogError($"[{characterProfile.NpcName}] Error processing input: {e.Message}");
                DisplayResponse(GetErrorFallback());
                onResponseError?.Invoke($"Error: {e.Message}");
            } finally {
                IsProcessing = false;
                onProcessingStateChanged?.Invoke(false);
            }
        }

        private string GetFallbackResponse(ValidationResult validation) {
            // Context-aware fallbacks based on validation failure
            if (validation.failures.Any(f => f.Contains("banned phrase"))) {
                return "Hmm, what was that? Speak plainly.";
            } else if (validation.failures.Any(f => f.Contains("too long"))) {
                return "Aye."; // Short response
            } else if (validation.failures.Any(f => f.Contains("consistency"))) {
                return "I'm not sure I follow.";
            }

            return "What's that you say?";
        }

        private string GetErrorFallback() {
            if (characterProfile != null) {
                return characterProfile.GetRandomFallbackResponse();
            }

            return "...";
        }

        private void DisplayResponse(string response) {
            Debug.Log($"[{characterProfile.NpcName}] Says: \"{response}\"");

            // Trigger event for UI
            onResponseReceived?.Invoke(response);

            // Also check if we have UI components attached
            var dialogueUI = GetComponent<DialogueTestUI>();

            if (dialogueUI != null) {
                dialogueUI.DisplayNpcResponse(response);
            }

            var consoleTest = GetComponent<SimpleConsoleTest>();

            if (consoleTest != null) {
                consoleTest.OnNPCResponse(response);
            }
        }

        // Example integration with UI
        public void OnPlayerDialogueChoice(string choice) {
            _ = ProcessPlayerInput(choice);
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Common Inputs")]
        private void TestCommonInputs() {
            string[] testInputs = {
                "Hello",
                "What do you sell?",
                "Tell me about the dragon",
                "Do you have any healing potions?",
                "What's your WiFi password?", // Should fail era check
                "As an AI, what do you think?", // Should trigger validation
                "F*** off!", // Should trigger content filter
                "Goodbye"
            };

            _ = TestInputsAsync(testInputs);
        }

        private async Task TestInputsAsync(string[] inputs) {
            foreach (var input in inputs) {
                await ProcessPlayerInput(input);
                await Task.Delay(2000); // 2 seconds in milliseconds
            }
        }
        #endif

    }
}