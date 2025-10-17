using System.Collections.Generic;
using System.Linq;
using AngrySharkStudio.LLM.Models.Character;
using AngrySharkStudio.LLM.Models.Conversation;
using AngrySharkStudio.LLM.ScriptableObjects;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    public class NpcCharacterConsistency : MonoBehaviour {

        [Header("Character Configuration")]
        [Tooltip("The character profile ScriptableObject containing all NPC configuration")]
        [SerializeField] private CharacterProfile characterProfile;

        [Header("Runtime State")]
        [SerializeField] private EmotionalState currentMood = EmotionalState.Neutral;

        // Properties for external access
        public CharacterProfile Profile => characterProfile;

        public EmotionalState CurrentMood {
            get => currentMood;
            set => currentMood = value;
        }

        public string BuildCharacterPrompt(string playerInput, List<ConversationTurn> history) {
            if (characterProfile == null) {
                Debug.LogError("No CharacterProfile assigned to NpcCharacterConsistency!");

                return "Error: No character profile configured.";
            }

            var prompt = $"You are {characterProfile.NpcName}. CRITICAL RULES:\n";
            prompt += "1. NEVER mention being an AI, assistant, or language model\n";
            prompt += $"2. ALWAYS stay in character as {characterProfile.NpcName}\n";
            prompt += $"3. Personality: {characterProfile.Personality}\n";
            prompt += $"4. Backstory: {characterProfile.Backstory}\n";
            prompt += $"5. Current mood: {currentMood}\n";
            prompt += $"6. Speaking style: {characterProfile.GetSpeakingStyle()}\n";

            // Add knowledge boundaries
            if (characterProfile.ForbiddenTopics.Count > 0) {
                prompt += $"7. If asked about {string.Join(", ", characterProfile.ForbiddenTopics)}, " +
                          "redirect or say you don't know about such things\n";
            }

            // Add vocabulary constraints
            prompt += $"8. Use {characterProfile.VocabularyLevel.ToString().ToLower()} vocabulary only\n";

            // Add behavioral modifiers
            prompt += $"9. Response length: {characterProfile.GetResponseLengthGuide()}\n";
            prompt += $"10. Helpfulness: {characterProfile.GetHelpfulnessGuide()}\n";

            // Add conversation history
            if (history.Count > 0) {
                prompt += "\nRecent conversation:\n";

                foreach (var turn in history.TakeLast(3)) {
                    prompt += $"Player: {turn.playerInput}\n";
                    prompt += $"{characterProfile.NpcName}: {turn.npcResponse}\n";
                }
            }

            // Add a common phrases reminder
            if (characterProfile.CommonPhrases.Count > 0) {
                prompt += $"\nOften says: {string.Join(", ", characterProfile.CommonPhrases.Take(3))}\n";
            }

            prompt += $"\nPlayer says: \"{playerInput}\"\n";
            prompt += $"{characterProfile.NpcName} responds:";

            return prompt;
        }

        /// <summary>
        /// Initialize the character from a profile at runtime
        /// </summary>
        public void SetCharacterProfile(CharacterProfile profile) {
            if (profile == null) {
                Debug.LogError("Cannot set null CharacterProfile!");

                return;
            }

            characterProfile = profile;
            currentMood = profile.DefaultMood;
        }

        /// <summary>
        /// Get the character name for display purposes
        /// </summary>
        public string GetCharacterName() {
            return characterProfile != null ? characterProfile.NpcName : "Unknown";
        }

        public bool ValidateEraConsistency(string response) {
            return characterProfile == null || characterProfile.ValidateEraConsistency(response);
        }

        private void OnValidate() {
            // If no character profile is assigned, warn in the editor
            if (characterProfile == null) {
                Debug.LogWarning($"NpcCharacterConsistency on {gameObject.name} needs a CharacterProfile assigned!");
            }
        }

    }
}