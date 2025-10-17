using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngrySharkStudio.LLM.PlatformDebuggers {
    public abstract class AiDebuggerBase : MonoBehaviour {

        [Header("Base Debug Settings")]
        [SerializeField] private bool enableDebugLogging = true;
        
        protected abstract string PlatformName { get; }

        // Platform-specific preprocessing
        protected abstract string PreprocessPrompt(string originalPrompt);

        // Platform-specific postprocessing
        protected abstract string PostprocessResponse(string rawResponse);

        // Platform-specific validation
        protected abstract bool ValidatePlatformSpecific(string response);

        // Metrics collection
        private int totalRequests;
        private int successfulRequests;
        private float totalResponseTime;
        private readonly List<float> responseTimes = new();

        public string ProcessAIRequest(string prompt, out float responseTime) {
            var startTime = Time.realtimeSinceStartup;
            totalRequests++;

            try {
                // Preprocess the prompt
                var processedPrompt = PreprocessPrompt(prompt);

                // Here you would make the actual API call
                // string rawResponse = await MakeAPICall(processedPrompt);
                var rawResponse = "[API Response would go here]";

                // Postprocess the response
                var processedResponse = PostprocessResponse(rawResponse);

                // Validate the response
                var isValid = ValidatePlatformSpecific(processedResponse);

                responseTime = Time.realtimeSinceStartup - startTime;
                totalResponseTime += responseTime;
                responseTimes.Add(responseTime);

                if (isValid) {
                    successfulRequests++;
                    LogDebug($"Successful response in {responseTime:F2}s");

                    return processedResponse;
                }

                LogDebug($"Response failed validation after {responseTime:F2}s");

                return GetFallbackResponse();
            } catch (Exception e) {
                LogError($"AI request failed: {e.Message}");
                responseTime = Time.realtimeSinceStartup - startTime;

                return GetFallbackResponse();
            }
        }

        protected virtual string GetFallbackResponse() {
            return "I don't quite understand what you're asking.";
        }

        protected virtual Dictionary<string, object> GetPlatformMetrics() {
            var metrics = new Dictionary<string, object> {
                ["platform"] = PlatformName,
                ["totalRequests"] = totalRequests,
                ["successfulRequests"] = successfulRequests,
                ["successRate"] = totalRequests > 0 ? (float)successfulRequests / totalRequests : 0f,
                ["averageResponseTime"] = responseTimes.Count > 0 ? totalResponseTime / responseTimes.Count : 0f,
                ["lastResponseTime"] = responseTimes.Count > 0 ? responseTimes[^1] : 0f
            };

            return metrics;
        }

        protected void LogDebug(string message) {
            if (enableDebugLogging) {
                Debug.Log($"[{PlatformName} Debug] {message}");
            }
        }

        protected void LogError(string message) {
            Debug.LogError($"[{PlatformName} Error] {message}");
        }

        public void ResetMetrics() {
            totalRequests = 0;
            successfulRequests = 0;
            totalResponseTime = 0f;
            responseTimes.Clear();
        }

    }
}