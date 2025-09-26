using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AngrySharkStudio.LLM.Examples {
    /// <summary>
    /// Simple UI controller for testing NPC dialogue system with TextMeshPro
    /// Beginner-friendly with clear comments and error handling
    /// </summary>
    public class DialogueTestUI : MonoBehaviour {

        [Header("UI Components")]
        [Tooltip("The input field where player types messages")]
        [SerializeField] private TMP_InputField playerInput;

        [Tooltip("The button to send messages")]
        [SerializeField] private Button sendButton;

        [Tooltip("Where NPC responses are displayed")]
        [SerializeField] private TextMeshProUGUI npcResponseText;

        [Tooltip("Shows 'Thinking...' while waiting for response")]
        [SerializeField] private GameObject loadingIndicator;

        [Header("NPC Connection")]
        [Tooltip("Drag your NPC GameObject with SmartNpcExample here")]
        [SerializeField] private SmartNpcExample npc;

        [Header("Settings")]
        [Tooltip("Clear input field after sending?")]
        [SerializeField] private bool clearInputAfterSend = true;

        [Tooltip("Show timestamp with responses?")]
        [SerializeField] private bool showTimestamp = true;

        [Tooltip("Automatically scroll to newest message?")]
        [SerializeField] private bool autoScroll = true;

        // Track if we're waiting for a response
        private bool isWaitingForResponse;

        private void Start() {
            InitializeUI();
        }

        /// <summary>
        /// Initialize the UI - can be called from editor scripts
        /// </summary>
        public void InitializeUI() {
            // Validate setup
            if (!ValidateSetup()) {
                return;
            }

            // Clear any existing listeners first
            sendButton.onClick.RemoveAllListeners();

            if (playerInput != null) {
                playerInput.onSubmit.RemoveAllListeners();
            }

            // Connect the sendButton
            sendButton.onClick.AddListener(SendMessage);

            // Optional: Send a message when Enter is pressed
            if (playerInput != null) {
                playerInput.onSubmit.AddListener(delegate { SendMessage(); });
            }

            // Hide loading indicator at start
            if (loadingIndicator != null) {
                loadingIndicator.SetActive(false);
            }

            // Show welcome message
            DisplayMessage("System", "NPC Dialogue Test UI Ready! Type a message and click Send.");
        }

        /// <summary>
        /// Validates that all required components are assigned
        /// </summary>
        private bool ValidateSetup() {
            var isValid = true;

            if (playerInput == null) {
                Debug.LogError(
                    "[DialogueTestUI] No input field assigned! Please assign playerInput (TMP_InputField) in the Inspector."
                );
                isValid = false;
            }

            if (npcResponseText == null) {
                Debug.LogError(
                    "[DialogueTestUI] No response text assigned! Please assign npcResponseText (TextMeshProUGUI) in the Inspector."
                );
                isValid = false;
            }

            if (sendButton == null) {
                Debug.LogError("[DialogueTestUI] No send button assigned! Please assign sendButton in the Inspector."
                );
                isValid = false;
            }

            if (npc != null) {
                return isValid;
            }

            Debug.LogError(
                "[DialogueTestUI] No NPC assigned! Please drag your NPC GameObject (with SmartNpcExample) to the npc field."
            );

            return false;
        }

        /// <summary>
        /// Send the current message to the NPC
        /// </summary>
        private void SendMessage() {
            // Don't send it if already waiting for response
            if (isWaitingForResponse) {
                Debug.LogWarning("[DialogueTestUI] Still waiting for previous response!");

                return;
            }

            // Get the input text
            var message = playerInput.text;

            // Don't send empty messages
            if (string.IsNullOrWhiteSpace(message)) {
                Debug.LogWarning("[DialogueTestUI] Cannot send empty message!");

                return;
            }

            // Show player message
            DisplayMessage("Player", message);

            // Clear input if enabled
            if (clearInputAfterSend) {
                playerInput.text = "";
                playerInput.ActivateInputField(); // Keep focus on input
            }

            // Send to NPC
            _ = SendMessageToNpcAsync(message);
        }

        /// <summary>
        /// Async method to handle NPC communication
        /// </summary>
        private async Task SendMessageToNpcAsync(string message) {
            isWaitingForResponse = true;

            // Disable send button while waiting
            if (sendButton != null) {
                sendButton.interactable = false;
            }

            // Show loading indicator
            if (loadingIndicator != null) {
                loadingIndicator.SetActive(true);
            }

            // Send message to NPC
            Debug.Log($"[DialogueTestUI] Sending to NPC: {message}");

            try {
                // Process the input
                await npc.ProcessPlayerInput(message);

                // SmartNpcExample will call DisplayNPCResponse when ready
            } catch (Exception e) {
                Debug.LogError($"[DialogueTestUI] Error: {e.Message}");
                DisplayMessage("System", $"Error: {e.Message}");

                // Reset UI state on error
                if (sendButton != null)
                    sendButton.interactable = true;

                if (loadingIndicator != null)
                    loadingIndicator.SetActive(false);

                isWaitingForResponse = false;
            }
        }

        /// <summary>
        /// Call this method from SmartNpcExample when the response is ready
        /// </summary>
        public void DisplayNpcResponse(string response) {
            DisplayMessage("NPC", response);

            // Hide loading indicator
            if (loadingIndicator != null) {
                loadingIndicator.SetActive(false);
            }

            // Re-enable sendButton
            if (sendButton != null) {
                sendButton.interactable = true;
            }

            isWaitingForResponse = false;
        }

        /// <summary>
        /// Display a message in the response area
        /// </summary>
        private void DisplayMessage(string speaker, string message) {
            var timestamp = showTimestamp ? $"[{DateTime.Now:HH:mm:ss}] " : "";
            var formattedMessage = $"{timestamp}<b>{speaker}:</b> {message}\n\n";

            // Add to display
            npcResponseText.text += formattedMessage;

            // Auto scroll if enabled
            if (!autoScroll) {
                return;
            }

            // Force layout rebuild and scroll to the bottom
            Canvas.ForceUpdateCanvases();

            if (npcResponseText.transform.parent == null) {
                return;
            }

            var scrollRect = npcResponseText.transform.parent.GetComponentInParent<ScrollRect>();

            if (scrollRect != null) {
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// Clear the conversation display
        /// </summary>
        [ContextMenu("Clear Conversation")]
        public void ClearConversation() {
            npcResponseText.text = "";
            DisplayMessage("System", "Conversation cleared.");
        }

        /// <summary>
        /// Send a test message
        /// </summary>
        [ContextMenu("Send Test Message")]
        public void SendTestMessage() {
            playerInput.text = "Hello, what do you sell?";
            SendMessage();
        }

        private void OnEnable() {
            if (npc == null) {
                return;
            }

            // Subscribe to NPC events if available
            npc.onResponseReceived.AddListener(DisplayNpcResponse);
            npc.onResponseError.AddListener(HandleError);
            npc.onProcessingStateChanged.AddListener(HandleProcessingState);
        }

        private void OnDisable() {
            if (npc == null) {
                return;
            }

            // Unsubscribe from NPC events
            npc.onResponseReceived.RemoveListener(DisplayNpcResponse);
            npc.onResponseError.RemoveListener(HandleError);
            npc.onProcessingStateChanged.RemoveListener(HandleProcessingState);
        }

        private void HandleError(string error) {
            DisplayMessage("Error", error);

            // Reset UI state
            if (sendButton != null) {
                sendButton.interactable = true;
            }

            if (loadingIndicator != null) {
                loadingIndicator.SetActive(false);
            }

            isWaitingForResponse = false;
        }

        private void HandleProcessingState(bool isProcessing) {
            isWaitingForResponse = isProcessing;

            if (sendButton != null) {
                sendButton.interactable = !isProcessing;
            }

            if (loadingIndicator != null) {
                loadingIndicator.SetActive(isProcessing);
            }
        }

    }
}