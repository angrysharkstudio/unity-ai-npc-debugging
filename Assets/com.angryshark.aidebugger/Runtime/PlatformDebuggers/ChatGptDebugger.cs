using System.Collections.Generic;
using UnityEngine;

namespace AngrySharkStudio.LLM.PlatformDebuggers {
    public class ChatGptDebugger : AiDebuggerBase {

        protected override string PlatformName => "ChatGPT";

        [Header("ChatGPT Specific Settings")]
        [SerializeField] private float temperature = 0.7f;
        [SerializeField] private int maxTokens = 150;
        [SerializeField] private float frequencyPenalty = 0.5f;
        [SerializeField] private float presencePenalty = 0.5f;

        protected override string PreprocessPrompt(string originalPrompt) {
            // ChatGPT specific preprocessing
            var processed = originalPrompt;

            // Add stronger character enforcement for ChatGPT
            processed = "IMPORTANT: You must ALWAYS stay in character. " +
                        "Never acknowledge being ChatGPT or an AI. " +
                        "Respond ONLY as the character described below.\n\n" +
                        processed;

            // Add response format constraints
            processed += "\n\nREMEMBER: Keep responses short and character-appropriate.";

            LogDebug($"ChatGPT Preprocessed prompt (temp={temperature}, tokens={maxTokens})");

            return processed;
        }

        protected override string PostprocessResponse(string rawResponse) {
            var processed = rawResponse;

            // Remove common ChatGPT artifacts
            processed = processed.Replace("As a language model,", "");
            processed = processed.Replace("I'm ChatGPT and", "");
            processed = processed.Replace("I don't have personal experiences, but", "");

            // Fix capitalization issues
            if (processed.Length > 0) {
                processed = char.ToUpper(processed[0]) + processed.Substring(1);
            }

            LogDebug("ChatGPT response postprocessed");

            return processed.Trim();
        }

        protected override bool ValidatePlatformSpecific(string response) {
            // ChatGPT-specific validation
            string[] chatGptArtifacts = {
                "I understand you're asking",
                "I'd be happy to help",
                "Is there anything else",
                "Feel free to ask"
            };

            foreach (var artifact in chatGptArtifacts) {
                if (!response.Contains(artifact)) {
                    continue;
                }

                LogDebug($"Found ChatGPT artifact: '{artifact}'");

                return false;
            }

            return true;
        }

        protected override Dictionary<string, object> GetPlatformMetrics() {
            var metrics = base.GetPlatformMetrics();
            metrics["temperature"] = temperature;
            metrics["maxTokens"] = maxTokens;
            metrics["frequencyPenalty"] = frequencyPenalty;
            metrics["presencePenalty"] = presencePenalty;

            return metrics;
        }

    }
}