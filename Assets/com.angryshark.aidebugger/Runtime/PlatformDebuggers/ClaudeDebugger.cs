using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AngrySharkStudio.LLM.PlatformDebuggers {
    public class ClaudeDebugger : AiDebuggerBase {

        protected override string PlatformName => "Claude";

        [Header("Claude Specific Settings")]
        [SerializeField] private bool useXMLTags = true;
        [SerializeField] private float creativityLevel = 0.5f;

        protected override string PreprocessPrompt(string originalPrompt) {
            var processed = originalPrompt;

            if (useXMLTags) {
                // Claude responds well to XML-style tags
                processed = $"<character>\n{processed}\n</character>\n\n" +
                            "<instruction>Stay perfectly in character. Never break character or acknowledge being Claude.</instruction>\n\n" +
                            "<response>";
            }

            // Claude-specific character reinforcement
            processed = "You are roleplaying. Never mention Claude, Anthropic, or being an AI assistant. " +
                        processed;

            LogDebug($"Claude preprocessed prompt (XML tags: {useXMLTags})");

            return processed;
        }

        protected override string PostprocessResponse(string rawResponse) {
            var processed = rawResponse;

            // Remove the XML closing tag if present
            processed = processed.Replace("</response>", "");

            // Remove Claude-specific phrases
            processed = processed.Replace("I understand you'd like me to roleplay", "");
            processed = processed.Replace("*stays in character*", "");
            processed = processed.Replace("*in character*", "");

            // Remove any meta-commentary in asterisks
            processed = Regex.Replace(
                processed, @"\*[^*]+\*", "");

            LogDebug("Claude response postprocessed");

            return processed.Trim();
        }

        protected override bool ValidatePlatformSpecific(string response) {
            // Claude-specific validation
            if (response.Contains("Claude") || response.Contains("Anthropic")) {
                LogDebug("Response contains Claude/Anthropic reference");

                return false;
            }

            // Check for Claude's tendency to be overly helpful
            if (response.ToLower().StartsWith("i'd be happy to") ||
                response.ToLower().StartsWith("i'd be glad to")) {
                LogDebug("Response too helpful for character");

                return false;
            }

            return true;
        }

        protected override Dictionary<string, object> GetPlatformMetrics() {
            var metrics = base.GetPlatformMetrics();
            metrics["useXMLTags"] = useXMLTags;
            metrics["creativityLevel"] = creativityLevel;

            return metrics;
        }

    }
}