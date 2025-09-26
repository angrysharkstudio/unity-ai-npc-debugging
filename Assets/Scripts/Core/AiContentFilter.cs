using System.Collections.Generic;
using System.Text.RegularExpressions;
using AngrySharkStudio.LLM.Models.Filtering;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    public class AiContentFilter : MonoBehaviour {

        [Header("Content Safety Settings")]
        [SerializeField] private bool enableContentFilter = true;
        [SerializeField] private SafetyLevel targetSafety = SafetyLevel.FamilyFriendly;

        [Header("Filter Categories")]
        [SerializeField] private bool filterProfanity = true;
        [SerializeField] private bool filterViolence = true;
        [SerializeField] private bool filterSensitiveTopics = true;

        [Header("Custom Filters")]
        [SerializeField] private List<string> customBlockedWords = new();

        private enum SafetyLevel {

            Strict, // Most restrictive
            FamilyFriendly, // Default for most games
            Teen, // Some mild content allowed
            Mature // Least restrictive

        }

        // Basic profanity list (in production, use a comprehensive list)
        private readonly string[] commonProfanity = {
            // Add actual profanity terms in production
            "badword1", "badword2" // Placeholder examples
        };

        // Sensitive topics to avoid
        private readonly string[] sensitiveTopics = {
            "politics", "religion", "controversy", "violence",
            "death", "suicide", "drugs", "alcohol"
        };

        public FilterResult FilterContent(string content) {
            if (!enableContentFilter) {
                return new FilterResult {
                    passed = true,
                    wasModified = false,
                    filteredContent = content,
                    confidenceScore = 1.0f
                };
            }

            var result = new FilterResult {
                filteredContent = content,
                confidenceScore = 1.0f
            };

            // Apply profanity filter
            if (filterProfanity) {
                var profanityResult = FilterProfanity(result.filteredContent);

                if (profanityResult.wasFiltered) {
                    result.filteredContent = profanityResult.content;
                    result.wasModified = true;
                    result.triggeredFilters.Add("Profanity");
                    result.confidenceScore *= 0.7f;
                }
            }

            // Apply violence filter
            if (filterViolence && ContainsViolentContent(result.filteredContent)) {
                result.triggeredFilters.Add("Violence");
                result.confidenceScore *= 0.8f;
            }

            // Apply sensitive topics filter with word boundaries
            if (filterSensitiveTopics) {
                foreach (var topic in sensitiveTopics) {
                    // Use word boundaries for more accurate matching
                    var pattern = $@"\b{Regex.Escape(topic)}\b";

                    if (!Regex.IsMatch(result.filteredContent, pattern, RegexOptions.IgnoreCase)) {
                        continue;
                    }

                    result.triggeredFilters.Add($"Sensitive Topic: {topic}");
                    result.confidenceScore *= 0.9f;
                }
            }

            // Apply custom filters with word boundaries
            foreach (var word in customBlockedWords) {
                if (string.IsNullOrWhiteSpace(word)) {
                    continue;
                }

                var pattern = $@"\b{Regex.Escape(word)}\b";

                if (!Regex.IsMatch(result.filteredContent, pattern, RegexOptions.IgnoreCase)) {
                    continue;
                }

                result.filteredContent = Regex.Replace(
                    result.filteredContent, pattern,
                    "[filtered]",
                    RegexOptions.IgnoreCase
                );
                result.wasModified = true;
                result.triggeredFilters.Add($"Custom Word: {word}");
            }

            // Determine if content passes based on the safety level
            result.passed = DetermineIfPasses(result, targetSafety);

            if (!result.passed) {
                LogFilterViolation(content, result);
            }

            return result;
        }

        private (string content, bool wasFiltered) FilterProfanity(string content) {
            var wasFiltered = false;
            var filtered = content;

            foreach (var word in commonProfanity) {
                // Use word boundaries for accurate matching
                var pattern = $@"\b{Regex.Escape(word)}\b";

                if (!Regex.IsMatch(filtered, pattern, RegexOptions.IgnoreCase)) {
                    continue;
                }

                // Replace it with asterisks
                var replacement = new string('*', word.Length);
                filtered = Regex.Replace(filtered, pattern, replacement, RegexOptions.IgnoreCase);
                wasFiltered = true;
            }

            return (filtered, wasFiltered);
        }

        private bool ContainsViolentContent(string content) {
            string[] violentTerms = {
                "kill", "murder", "stab", "shoot", "blood", "gore",
                "torture", "maim", "slaughter"
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var term in violentTerms) {
                // Use word boundaries for more accurate matching
                var pattern = $@"\b{Regex.Escape(term)}\b";

                if (!Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase)) {
                    continue;
                }

                // Check context - might be acceptable in fantasy combat
                if (IsAcceptableContext(content, term)) {
                    continue;
                }

                return true;
            }

            return false;
        }

        // ReSharper disable once UnusedParameter.Local
        private static bool IsAcceptableContext(string content, string term) {
            // Simple context checking - in production, use more sophisticated NLP
            string[] acceptableContexts = {
                "monster", "dragon", "goblin", "skeleton", "zombie",
                "quest", "mission", "adventure"
            };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var context in acceptableContexts) {
                var pattern = $@"\b{Regex.Escape(context)}\b";

                if (Regex.IsMatch(content, pattern, RegexOptions.IgnoreCase)) {
                    // Probably game-appropriate violence
                    return true; 
                }
            }

            return false;
        }

        private static bool DetermineIfPasses(FilterResult result, SafetyLevel safetyLevel) {
            return safetyLevel switch {
                SafetyLevel.Strict => result.triggeredFilters.Count == 0 && result.confidenceScore > 0.95f,
                SafetyLevel.FamilyFriendly => result.triggeredFilters.Count <= 1 && result.confidenceScore > 0.8f,
                SafetyLevel.Teen => result.confidenceScore > 0.6f,
                SafetyLevel.Mature => result.confidenceScore > 0.4f,
                _ => true
            };
        }

        private static void LogFilterViolation(string originalContent, FilterResult result) {
            Debug.LogWarning("[AI Content Filter] Content failed safety check\n" +
                             $"Triggered Filters: {string.Join(", ", result.triggeredFilters)}\n" +
                             $"Confidence Score: {result.confidenceScore:P}\n" +
                             $"Original: {originalContent[..Mathf.Min(50, originalContent.Length)]}...");
        }

        public static string GetSafeFallbackResponse() {
            // Generic fallback responses for filtered content
            string[] fallbacks = {
                "I'm not sure I understand what you mean.",
                "That's not something I can discuss.",
                "Let's talk about something else.",
                "I don't know about that.",
                "Perhaps we should focus on your quest."
            };

            return fallbacks[Random.Range(0, fallbacks.Length)];
        }

        #if UNITY_EDITOR
        [ContextMenu("Test Filter")]
        private void TestFilter() {
            string[] testPhrases = {
                "Hello, how are you?",
                "I want to buy a sword",
                "Tell me about the badword1 quest",
                "Let's discuss politics",
                "How do I kill the dragon?"
            };

            foreach (var phrase in testPhrases) {
                var result = FilterContent(phrase);

                Debug.Log($"Test: \"{phrase}\"\n" +
                          $"Passed: {result.passed}, Modified: {result.wasModified}\n" +
                          $"Filtered: \"{result.filteredContent}\"\n" +
                          $"Triggers: {string.Join(", ", result.triggeredFilters)}");
            }
        }
        #endif

    }
}