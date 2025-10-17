using System;
using System.Collections.Generic;
using System.IO;
using AngrySharkStudio.LLM.Models.Debug;
using UnityEditor;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    public class AiResponseDebugger : MonoBehaviour {
        private const int MaxHistorySize = 100;
        
        
        [Header("Debug Settings")]
        [SerializeField] private bool logAllRequests = true;
        [SerializeField] private bool logValidationFailures = true;
        [SerializeField] private bool enableVisualDebugger = true;

        [Header("Validation Rules")]
        [SerializeField] private List<string> bannedPhrases = new() {
            "as an AI", "language model", "I cannot", "I'm sorry",
            "I apologize", "virtual assistant"
        };

        [Header("Character Settings")]
        [SerializeField] private string characterName;
        [SerializeField] private List<string> allowedTopics;
        [SerializeField] private int maxResponseLength = 200;

        private readonly Queue<DebugEntry> debugHistory = new();
        

        public ValidationResult ValidateResponse(string response, string characterContext) {
            var result = new ValidationResult { passed = true };

            // Check for banned phrases
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var banned in bannedPhrases) {
                if (!response.ToLower().Contains(banned.ToLower())) {
                    continue;
                }

                result.passed = false;
                result.failures.Add($"Contains banned phrase: '{banned}'");
            }

            // Check response length
            if (response.Length > maxResponseLength) {
                result.passed = false;
                result.failures.Add($"Response too long: {response.Length} chars (max: {maxResponseLength})");
            }

            // Check character consistency
            result.consistencyScore = CalculateConsistencyScore(response, characterContext);

            if (result.consistencyScore < 0.7f) {
                result.passed = false;
                result.failures.Add($"Low consistency score: {result.consistencyScore:P}");
            }

            // Log validation failures
            if (logValidationFailures && !result.passed) {
                Debug.LogWarning($"[AI Validation Failed] {characterName}: {string.Join(", ", result.failures)}");
            }

            return result;
        }

        private static float CalculateConsistencyScore(string response, string context) {
            // Implement scoring logic based on:
            // - Vocabulary matching
            // - Tone consistency
            // - Topic relevance
            
            // This is a simplified version
            var score = 1.0f;

            // Check for modern references in fantasy setting
            string[] modernTerms = { "GPS", "internet", "computer", "smartphone", "email" };

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var term in modernTerms) {
                if (response.ToLower().Contains(term.ToLower())) {
                    score -= 0.3f;
                }
            }

            return Mathf.Clamp01(score);
        }

        public void LogDebugEntry(string playerInput, string aiPrompt, string rawResponse,
            string finalResponse, ValidationResult validation,
            float responseTime, string apiUsed) {
            var entry = new DebugEntry {
                timestamp = Time.time.ToString("F2"),
                playerInput = playerInput,
                aiPrompt = aiPrompt,
                rawResponse = rawResponse,
                finalResponse = finalResponse,
                validation = validation,
                responseTime = responseTime,
                apiUsed = apiUsed
            };

            debugHistory.Enqueue(entry);

            // Maintain history size
            while (debugHistory.Count > MaxHistorySize) {
                debugHistory.Dequeue();
            }

            if (logAllRequests) {
                Debug.Log($"[AI Debug] {characterName} | Player: \"{playerInput}\" | " +
                          $"Response Time: {responseTime:F2}s | API: {apiUsed}");
            }
        }

        public Queue<DebugEntry> GetDebugHistory() {
            return new Queue<DebugEntry>(debugHistory);
        }

        /// <summary>
        /// Sets the character name for debug logging
        /// </summary>
        // ReSharper disable once ParameterHidesMember
        public void SetCharacterName(string characterName) {
            this.characterName = characterName;
        }

        /// <summary>
        /// Sets the maximum allowed response length
        /// </summary>
        public void SetMaxResponseLength(int maxLength) {
            maxResponseLength = maxLength;
        }

        /// <summary>
        /// Sets the list of banned phrases to check for
        /// </summary>
        public void SetBannedPhrases(List<string> phrases) {
            bannedPhrases = phrases ?? new List<string>();
        }

        public void ExportDebugLog() {
            var path = Application.persistentDataPath +
                       $"/ai_debug_{characterName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var json = JsonUtility.ToJson(new DebugLog { entries = new List<DebugEntry>(debugHistory) }, true);
            File.WriteAllText(path, json);
            Debug.Log($"Debug log exported to: {path}");
        }

        [Serializable]
        private class DebugLog {

            public List<DebugEntry> entries;

        }

        // Editor visualization
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected() {
            if (!enableVisualDebugger) return;

            // Draw debug visualization in the scene view
            var style = new GUIStyle {
                normal = {
                    textColor = Color.yellow
                }
            };

            Handles.Label(transform.position + Vector3.up * 2,
                $"AI Debug: {characterName}\nHistory: {debugHistory.Count} entries", style);
        }
        #endif

    }
}