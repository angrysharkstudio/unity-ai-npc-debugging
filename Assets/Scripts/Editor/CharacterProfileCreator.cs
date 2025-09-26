using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AngrySharkStudio.LLM.Models.Character;
using AngrySharkStudio.LLM.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace AngrySharkStudio.LLM.Editor {
    /// <summary>
    /// Editor utilities for creating and managing CharacterProfile ScriptableObjects
    /// </summary>
    public static class CharacterProfileCreator {

        private const string DefaultProfilePath = "Assets/AI NPC Profiles/";

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Create Character Profile Asset", false, 20)]
        public static void CreateCharacterProfileAsset() {
            CreateCharacterProfileWithPreset("New Character", CharacterPreset.Custom);
        }

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Character Profiles/Gruff Merchant", false, 21)]
        public static void CreateGruffMerchantProfile() {
            CreateCharacterProfileWithPreset("Gareth the Merchant", CharacterPreset.GruffMerchant);
        }

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Character Profiles/Wise Scholar", false, 22)]
        public static void CreateWiseScholarProfile() {
            CreateCharacterProfileWithPreset("Elena the Scholar", CharacterPreset.WiseScholar);
        }

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Character Profiles/Suspicious Guard", false, 23)]
        public static void CreateSuspiciousGuardProfile() {
            CreateCharacterProfileWithPreset("Boris the Guard", CharacterPreset.SuspiciousGuard);
        }

        private enum CharacterPreset {

            Custom,
            GruffMerchant,
            WiseScholar,
            SuspiciousGuard

        }

        private static void CreateCharacterProfileWithPreset(string fileName, CharacterPreset preset) {
            // Ensure directory exists
            if (!Directory.Exists(DefaultProfilePath)) {
                Directory.CreateDirectory(DefaultProfilePath);
            }

            // Create the ScriptableObject instance
            var profile = ScriptableObject.CreateInstance<CharacterProfile>();

            // Apply preset values using reflection (since fields are private)
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            switch (preset) {
                case CharacterPreset.GruffMerchant:
                    SetPrivateField(profile, "npcName", "Gareth the Merchant", bindingFlags);

                    SetPrivateField(profile, "personality",
                        "Gruff but fair merchant, suspicious of strangers but warms up to regular customers. Former adventurer who knows the value of good equipment.",
                        bindingFlags);

                    SetPrivateField(profile, "backstory",
                        "Former adventurer who retired after a close call with a dragon. Now runs a modest shop in the market district, selling weapons and armor to those brave enough to face the dangers outside the city walls.",
                        bindingFlags);
                    SetPrivateField(profile, "vocabularyLevel", VocabularyLevel.Medieval, bindingFlags);
                    SetPrivateField(profile, "characterEra", Era.Fantasy, bindingFlags);
                    SetPrivateField(profile, "defaultMood", EmotionalState.Suspicious, bindingFlags);
                    SetPrivateField(profile, "formalityLevel", 0.3f, bindingFlags);
                    SetPrivateField(profile, "helpfulness", 0.6f, bindingFlags);
                    SetPrivateField(profile, "verbosity", 0.4f, bindingFlags);

                    SetPrivateList(profile, "commonPhrases", new[] {
                        "What can I do for ye?",
                        "Gold up front, no exceptions.",
                        "Aye, that'll do.",
                        "Been in the shop business for twenty years now.",
                        "You break it, you buy it.",
                        "That's quality steel, that is."
                    }, bindingFlags);

                    SetPrivateList(profile, "knownTopics", new[] {
                        "weapons", "armor", "potions", "local rumors",
                        "dragon incident", "shop inventory", "adventuring gear",
                        "market prices", "city guards", "thieves guild"
                    }, bindingFlags);

                    SetPrivateList(profile, "forbiddenTopics", new[] {
                        "modern technology", "cars", "internet", "smartphones",
                        "computers", "television", "electricity"
                    }, bindingFlags);

                    SetPrivateList(profile, "fallbackResponses", new[] {
                        "*grunts*",
                        "Eh? Speak up.",
                        "Can't hear you over the clanging.",
                        "What now?",
                        "Make it quick, I've got inventory to sort."
                    }, bindingFlags);

                    break;

                case CharacterPreset.WiseScholar:
                    SetPrivateField(profile, "npcName", "Elena the Scholar", bindingFlags);

                    SetPrivateField(profile, "personality",
                        "Knowledgeable and patient scholar who loves sharing wisdom. Speaks in measured tones and often quotes ancient texts. Can be long-winded when discussing favorite subjects.",
                        bindingFlags);

                    SetPrivateField(profile, "backstory",
                        "Head librarian at the Grand Archives for three decades. Has read every book in the collection at least twice. Specializes in ancient history and magical theory.",
                        bindingFlags);
                    SetPrivateField(profile, "vocabularyLevel", VocabularyLevel.Academic, bindingFlags);
                    SetPrivateField(profile, "characterEra", Era.Fantasy, bindingFlags);
                    SetPrivateField(profile, "defaultMood", EmotionalState.Friendly, bindingFlags);
                    SetPrivateField(profile, "formalityLevel", 0.8f, bindingFlags);
                    SetPrivateField(profile, "helpfulness", 0.9f, bindingFlags);
                    SetPrivateField(profile, "verbosity", 0.8f, bindingFlags);

                    SetPrivateList(profile, "commonPhrases", new[] {
                        "Ah, an excellent question!",
                        "As the ancient texts say...",
                        "Knowledge is the greatest treasure.",
                        "Let me consult my notes.",
                        "Have you considered the historical context?",
                        "Fascinating topic, truly fascinating."
                    }, bindingFlags);

                    SetPrivateList(profile, "knownTopics", new[] {
                        "history", "magic theory", "ancient civilizations", "prophecies",
                        "rare books", "languages", "philosophy", "alchemy basics",
                        "library rules", "research methods"
                    }, bindingFlags);

                    break;

                case CharacterPreset.SuspiciousGuard:
                    SetPrivateField(profile, "npcName", "Boris the Guard", bindingFlags);

                    SetPrivateField(profile, "personality",
                        "Vigilant city guard who takes his job very seriously. Suspicious of everyone, especially adventurers. Follows rules to the letter and has no sense of humor while on duty.",
                        bindingFlags);

                    SetPrivateField(profile, "backstory",
                        "Twenty-year veteran of the city watch. Has seen every trick in the book and trusts no one. Lost his partner to a rogue's blade five years ago and has been extra cautious ever since.",
                        bindingFlags);
                    SetPrivateField(profile, "vocabularyLevel", VocabularyLevel.Simple, bindingFlags);
                    SetPrivateField(profile, "characterEra", Era.Fantasy, bindingFlags);
                    SetPrivateField(profile, "defaultMood", EmotionalState.Suspicious, bindingFlags);
                    SetPrivateField(profile, "formalityLevel", 0.5f, bindingFlags);
                    SetPrivateField(profile, "helpfulness", 0.3f, bindingFlags);
                    SetPrivateField(profile, "verbosity", 0.3f, bindingFlags);

                    SetPrivateList(profile, "commonPhrases", new[] {
                        "Move along.",
                        "No loitering.",
                        "State your business.",
                        "I've got my eye on you.",
                        "Papers, please.",
                        "That's city property."
                    }, bindingFlags);

                    SetPrivateList(profile, "knownTopics", new[] {
                        "city laws", "guard duties", "suspicious activities",
                        "patrol routes", "criminal types", "city gates",
                        "curfew hours", "permits", "guard captain"
                    }, bindingFlags);

                    break;
            }

            // Create the asset
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{DefaultProfilePath}{fileName}.asset");
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the newly created asset
            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);

            Debug.Log($"Created CharacterProfile asset at: {assetPath}");
        }

        private static void SetPrivateField(object obj, string fieldName, object value, BindingFlags bindingFlags) {
            var field = obj.GetType().GetField(fieldName, bindingFlags);

            if (field != null) {
                field.SetValue(obj, value);
            }
        }

        private static void SetPrivateList(object obj, string fieldName, string[] values, BindingFlags bindingFlags) {
            var field = obj.GetType().GetField(fieldName, bindingFlags);

            if (field == null) {
                return;
            }

            var list = new List<string>(values);
            field.SetValue(obj, list);
        }

    }
}