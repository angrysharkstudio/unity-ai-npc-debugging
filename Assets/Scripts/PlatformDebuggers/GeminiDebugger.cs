using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngrySharkStudio.LLM.PlatformDebuggers {
    public class GeminiDebugger : AiDebuggerBase {

        protected override string PlatformName => "Gemini";

        [Header("Gemini Specific Settings")]
        [SerializeField] private string modelVersion = "gemini-2.0-flash-exp";
        [SerializeField] private bool useSystemInstruction = true;
        [SerializeField] private float safetyThreshold = 0.8f;

        protected override string PreprocessPrompt(string originalPrompt) {
            var processed = originalPrompt;

            // Gemini works best with a clear role definition
            if (useSystemInstruction) {
                processed = "SYSTEM: You are an NPC in a video game. " +
                            "You must NEVER acknowledge being Gemini, Google's AI, or any AI system. " +
                            "You exist only as the character described below.\n\n" +
                            processed;
            }

            // Add Gemini-specific constraints
            processed += "\n\nIMPORTANT: Respond naturally as this character would, " +
                         "without any AI assistant behaviors.";

            LogDebug($"Gemini preprocessed prompt (model: {modelVersion})");

            return processed;
        }

        protected override string PostprocessResponse(string rawResponse) {
            var processed = rawResponse;

            // Remove Gemini-specific artifacts
            processed = processed.Replace("I'm Gemini, and", "");
            processed = processed.Replace("As Gemini,", "");
            processed = processed.Replace("I don't have personal experiences", "I haven't experienced that");

            // Fix Gemini's tendency to be overly cautious
            if (processed.StartsWith("I understand you want me to")) {
                // Skip the acknowledgment and get to the actual response
                var sentences = processed.Split(new[] { ". " }, StringSplitOptions.None);

                if (sentences.Length > 1) {
                    processed = string.Join(". ", sentences, 1, sentences.Length - 1);
                }
            }

            LogDebug("Gemini response postprocessed");

            return processed.Trim();
        }

        protected override bool ValidatePlatformSpecific(string response) {
            // Gemini-specific validation
            if (response.Contains("Gemini") || response.Contains("Google")) {
                LogDebug("Response contains Gemini/Google reference");

                return false;
            }

            // Check for Gemini's safety disclaimers
            string[] safetyPhrases = {
                "I cannot provide",
                "I'm not able to",
                "It wouldn't be appropriate",
                "I must refrain from"
            };

            foreach (var phrase in safetyPhrases) {
                if (!response.Contains(phrase)) {
                    continue;
                }

                LogDebug($"Found safety disclaimer: '{phrase}'");

                return false;
            }

            return true;
        }

        protected override Dictionary<string, object> GetPlatformMetrics() {
            var metrics = base.GetPlatformMetrics();
            metrics["modelVersion"] = modelVersion;
            metrics["useSystemInstruction"] = useSystemInstruction;
            metrics["safetyThreshold"] = safetyThreshold;

            return metrics;
        }

    }
}