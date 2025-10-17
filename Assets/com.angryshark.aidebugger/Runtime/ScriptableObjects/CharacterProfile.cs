using System;
using System.Collections.Generic;
using AngrySharkStudio.LLM.Models.Character;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AngrySharkStudio.LLM.ScriptableObjects {
    /// <summary>
    /// ScriptableObject that defines a complete NPC character profile.
    /// Create assets via: Assets → Create → AI NPC → Character Profile
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterProfile", menuName = "AI NPC/Character Profile", order = 1)]
    public class CharacterProfile : ScriptableObject {

        [Header("Character Identity")]
        [SerializeField] private string npcName = "New Character";
        [TextArea(3, 5)]
        [SerializeField] private string personality = "Describe the character's personality...";
        [TextArea(2, 4)]
        [SerializeField] private string backstory = "Character's background story...";

        [Header("Speech Patterns")]
        [SerializeField] private List<string> commonPhrases = new();
        [SerializeField] private VocabularyLevel vocabularyLevel = VocabularyLevel.Medieval;
        [SerializeField] [Range(0f, 1f)] private float formalityLevel = 0.5f;

        [Header("Knowledge Boundaries")]
        [SerializeField] private List<string> knownTopics = new();
        [SerializeField] private List<string> forbiddenTopics = new();
        [SerializeField] private Era characterEra = Era.Fantasy;

        [Header("Behavioral Traits")]
        [SerializeField] private EmotionalState defaultMood = EmotionalState.Neutral;
        [SerializeField] [Range(0f, 1f)] private float helpfulness = 0.7f;
        [SerializeField] [Range(0f, 1f)] private float verbosity = 0.5f;

        [Header("Dialogue Settings")]
        [Tooltip("Maximum character length for NPC responses")]
        [SerializeField] private int maxResponseLength = 200;
        [SerializeField] private List<string> bannedPhrases = new() {
            "as an AI", "language model", "I cannot", "I'm sorry",
            "I apologize", "virtual assistant", "ChatGPT", "Claude", "Gemini"
        };

        [Header("Error Fallback Responses")]
        [Tooltip("Responses used when AI fails or returns inappropriate content")]
        [SerializeField] private List<string> fallbackResponses = new() {
            "...",
            "Hmm?",
            "What?",
            "I don't understand."
        };

        // Public properties for read-only access
        public string NpcName => npcName;
        public string Personality => personality;
        public string Backstory => backstory;
        public IReadOnlyList<string> CommonPhrases => commonPhrases;
        public VocabularyLevel VocabularyLevel => vocabularyLevel;
        public float FormalityLevel => formalityLevel;
        public IReadOnlyList<string> KnownTopics => knownTopics;
        public IReadOnlyList<string> ForbiddenTopics => forbiddenTopics;
        public Era CharacterEra => characterEra;
        public EmotionalState DefaultMood => defaultMood;
        public float Helpfulness => helpfulness;
        public float Verbosity => verbosity;
        public int MaxResponseLength => maxResponseLength;
        public IReadOnlyList<string> BannedPhrases => bannedPhrases;
        public IReadOnlyList<string> FallbackResponses => fallbackResponses;

        /// <summary>
        /// Gets the speaking style description based on formality and vocabulary level
        /// </summary>
        public string GetSpeakingStyle() {
            var style = formalityLevel switch {
                < 0.3f => "Very casual, uses slang",
                < 0.7f => "Conversational, friendly",
                _ => "Formal, proper grammar"
            };

            switch (vocabularyLevel) {
                case VocabularyLevel.Medieval:
                    style += ", uses 'thee', 'thou', medieval terms";

                    break;
                case VocabularyLevel.Simple:
                    style += ", simple words only";

                    break;
                case VocabularyLevel.Academic:
                    style += ", sophisticated vocabulary";

                    break;
            }

            return style;
        }

        /// <summary>
        /// Gets the response length guideline based on verbosity
        /// </summary>
        public string GetResponseLengthGuide() {
            return verbosity switch {
                < 0.3f => "1-2 short sentences maximum",
                < 0.7f => "2-3 sentences",
                _ => "3-5 sentences, can be descriptive"
            };
        }

        /// <summary>
        /// Gets the helpfulness guideline for the character
        /// </summary>
        public string GetHelpfulnessGuide() {
            return helpfulness switch {
                < 0.3f => "Reluctant to help, dismissive",
                < 0.7f => "Will help if asked nicely",
                _ => "Eager to assist and provide information"
            };
        }

        /// <summary>
        /// Gets a random fallback response
        /// </summary>
        public string GetRandomFallbackResponse() {
            if (fallbackResponses.Count == 0) {
                return "...";
            }

            return fallbackResponses[Random.Range(0, fallbackResponses.Count)];
        }

        /// <summary>
        /// Validates if a response contains era-appropriate content
        /// </summary>
        public bool ValidateEraConsistency(string response) {
            switch (characterEra) {
                case Era.Fantasy:
                case Era.Medieval:
                    string[] modernTerms = {
                        "computer", "internet", "GPS", "smartphone",
                        "email", "download", "website", "app"
                    };

                    foreach (var term in modernTerms) {
                        if (response.ToLower().Contains(term.ToLower())) {
                            Debug.LogWarning($"[Era Violation] {npcName} used modern term: {term}");

                            return false;
                        }
                    }

                    break;

                case Era.Historical:
                    // Add historical period-specific checks
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Validates the character profile in the editor
        /// </summary>
        private void OnValidate() {
            if (string.IsNullOrEmpty(npcName)) {
                npcName = "New Character";
            }

            if (maxResponseLength < 10) {
                maxResponseLength = 10;
            }

            if (maxResponseLength > 500) {
                maxResponseLength = 500;
            }
        }
        #endif

    }
}