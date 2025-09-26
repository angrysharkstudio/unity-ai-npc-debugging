using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AngrySharkStudio.LLM.Models.Configuration;
using AngrySharkStudio.LLM.Models.Requests;
using AngrySharkStudio.LLM.Models.Responses;
using UnityEngine;
using UnityEngine.Networking;

namespace AngrySharkStudio.LLM.API {
    /// <summary>
    /// LLM Manager for AI NPC Debugging - handles secure API communication
    /// Based on the secure configuration system from llm-examples
    /// </summary>
    public class LlmManager : MonoBehaviour {

        // Singleton pattern
        public static LlmManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        // Configuration from the JSON file - no hardcoded API keys!
        private string apiKey = "";
        private AIProvider provider = AIProvider.OpenAI;
        private string model = "gpt-3.5-turbo";

        private APIConfiguration config;
        private ProviderConfig currentProviderConfig;

        private enum AIProvider {

            OpenAI,
            Claude,
            Gemini

        }

        private void Awake() {
            // Set up singleton
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                LoadConfiguration();
            } else {
                Destroy(gameObject);
            }
        }

        // Load configuration from an external file
        private void LoadConfiguration() {
            try {
                var path = Path.Combine(Application.dataPath, "../api-config.json");

                if (File.Exists(path)) {
                    var json = File.ReadAllText(path);
                    config = JsonUtility.FromJson<APIConfiguration>(json);

                    LoadProviderConfig();

                    if (showDebugLogs) {
                        Debug.Log("[LLMManager] Configuration loaded from api-config.json");
                    }
                } else {
                    Debug.LogWarning("[LLMManager] WARNING: No api-config.json found. Create one next to Assets folder."
                    );
                    Debug.LogWarning("[LLMManager] See documentation for api-config.json format.");

                    config = new APIConfiguration();
                    currentProviderConfig = new ProviderConfig();
                }
            } catch (Exception e) {
                Debug.LogError($"[LLMManager] ERROR loading configuration: {e.Message}");
                config = new APIConfiguration();
                currentProviderConfig = new ProviderConfig();
            }
        }

        private void LoadProviderConfig() {
            var activeProvider = config.activeProvider;

            if (!string.IsNullOrEmpty(activeProvider)) {
                provider = activeProvider.ToLower() switch {
                    "openai" => AIProvider.OpenAI,
                    "claude" => AIProvider.Claude,
                    "gemini" => AIProvider.Gemini,
                    _ => AIProvider.OpenAI
                };
            }

            currentProviderConfig = provider switch {
                AIProvider.OpenAI => config.providers.openai,
                AIProvider.Claude => config.providers.claude,
                AIProvider.Gemini => config.providers.gemini,
                _ => new ProviderConfig()
            };

            if (currentProviderConfig == null) {
                return;
            }

            if (!string.IsNullOrEmpty(currentProviderConfig.apiKey)) {
                apiKey = currentProviderConfig.apiKey;
            }

            if (!string.IsNullOrEmpty(currentProviderConfig.model)) {
                model = currentProviderConfig.model;
            }
        }

        /// <summary>
        /// Send a message to the AI with character context
        /// </summary>
        public async Task<string> GetAIResponse(string prompt, float temperature = 0.7f) {
            if (string.IsNullOrEmpty(apiKey)) {
                Debug.LogError("[LLMManager] No API key! Create api-config.json file.");

                return "Error: No API key configured";
            }

            try {
                var url = GetAPIEndpoint();
                var jsonBody = CreateRequestBody(prompt, temperature);

                using var request = new UnityWebRequest(url, "POST");

                var bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");
                SetAuthHeaders(request);

                var operation = request.SendWebRequest();

                while (!operation.isDone) {
                    await Task.Yield();
                }

                if (request.result != UnityWebRequest.Result.Success) {
                    Debug.LogError($"[LLMManager] API Error: {request.error}");
                    Debug.LogError($"[LLMManager] Response: {request.downloadHandler.text}");

                    return $"API Error: {request.error}";
                }

                var response = ParseResponse(request.downloadHandler.text);

                if (showDebugLogs) {
                    Debug.Log($"[LLMManager] Response received ({response.Length} chars)");
                }

                return response;
            } catch (Exception e) {
                Debug.LogError($"[LLMManager] Exception: {e.Message}");

                return $"Error: {e.Message}";
            }
        }

        private string GetAPIEndpoint() {
            return provider switch {
                // ReSharper disable once DuplicatedSwitchExpressionArms
                AIProvider.OpenAI => currentProviderConfig.apiUrl,
                AIProvider.Claude => currentProviderConfig.apiUrl,
                AIProvider.Gemini => $"{currentProviderConfig.apiUrl}{model}:generateContent?key={apiKey}",
                _ => ""
            };
        }

        private void SetAuthHeaders(UnityWebRequest request) {
            switch (provider) {
                case AIProvider.OpenAI:
                    request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

                    break;
                case AIProvider.Claude:
                    request.SetRequestHeader("x-api-key", apiKey);
                    request.SetRequestHeader("anthropic-version", currentProviderConfig.apiVersion);

                    break;
                case AIProvider.Gemini:
                    // API key is in URL for Gemini
                    break;
                default:
                    throw new Exception("Unknown AI provider");
            }
        }

        private string CreateRequestBody(string prompt, float temperature) {
            return provider switch {
                AIProvider.OpenAI => CreateOpenAIRequest(prompt, temperature),
                AIProvider.Claude => CreateClaudeRequest(prompt, temperature),
                AIProvider.Gemini => CreateGeminiRequest(prompt, temperature),
                _ => ""
            };
        }

        private string CreateOpenAIRequest(string prompt, float temperature) {
            var requestObj = new OpenAiRequest {
                model = model,
                messages = new List<OpenAiMessage> {
                    new() { role = "system", content = prompt }
                },
                max_tokens = config.globalSettings.maxTokens,
                temperature = temperature
            };

            return JsonUtility.ToJson(requestObj);
        }

        private string CreateClaudeRequest(string prompt, float temperature) {
            var requestObj = new ClaudeRequest {
                model = model,
                messages = new List<ClaudeMessage> {
                    new() { role = "user", content = prompt }
                },
                max_tokens = config.globalSettings.maxTokens,
                temperature = temperature
            };

            return JsonUtility.ToJson(requestObj);
        }

        private string CreateGeminiRequest(string prompt, float temperature) {
            var requestObj = new GeminiRequest {
                contents = new List<GeminiContent> {
                    new() {
                        parts = new List<GeminiPart> {
                            new() { text = prompt }
                        }
                    }
                },
                generationConfig = new GeminiConfig {
                    temperature = temperature,
                    maxOutputTokens = config.globalSettings.maxTokens
                }
            };

            return JsonUtility.ToJson(requestObj);
        }

        private string ParseResponse(string json) {
            try {
                return provider switch {
                    AIProvider.OpenAI => ParseOpenAIResponse(json),
                    AIProvider.Claude => ParseClaudeResponse(json),
                    AIProvider.Gemini => ParseGeminiResponse(json),
                    _ => "Unknown provider"
                };
            } catch (Exception e) {
                Debug.LogError($"[LLMManager] Failed to parse response: {e.Message}");
                Debug.LogError($"[LLMManager] Raw response: {json}");

                return "Error parsing response.";
            }
        }

        private static string ParseOpenAIResponse(string json) {
            var response = JsonUtility.FromJson<OpenAiResponse>(json);

            if (response?.choices is { Count: > 0 }) {
                return response.choices[0].message.content;
            }

            return "No response content";
        }

        private static string ParseClaudeResponse(string json) {
            var response = JsonUtility.FromJson<ClaudeResponse>(json);

            if (response?.content is { Count: > 0 }) {
                return response.content[0].text;
            }

            return "No response content";
        }

        private static string ParseGeminiResponse(string json) {
            var response = JsonUtility.FromJson<GeminiResponse>(json);

            if (response?.candidates is not { Count: > 0 }) {
                return "No response content";
            }

            var content = response.candidates[0].content;

            if (content?.parts is { Count: > 0 }) {
                return content.parts[0].text;
            }

            return "No response content";
        }

    }
}