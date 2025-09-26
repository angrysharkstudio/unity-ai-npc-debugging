using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngrySharkStudio.LLM.Models.Cache;
using AngrySharkStudio.LLM.Models.Character;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    public class AiResponseCache : MonoBehaviour {

        [Header("Cache Settings")]
        [Tooltip("Enable or disable response caching")]
        [SerializeField] private bool enableCaching = true;

        [Tooltip("Maximum number of cached responses to store")]
        [SerializeField] private int maxCacheSize = 100;

        [Tooltip("How many hours before cached responses expire")]
        [SerializeField] private float cacheExpirationHours = 24f;

        [Tooltip("Minimum similarity score (0-1) to consider a cached response valid")]
        [SerializeField] [Range(0f, 1f)] private float similarityThreshold = 0.85f;

        [Header("Consistency Settings")]
        [SerializeField] private bool enforceTemporal = true;
        [SerializeField] private bool enforceEmotional = true;

        private readonly Dictionary<string, CacheEntry> responseCache = new();
        private readonly Queue<string> cacheOrder = new();

        public string GetCachedResponse(string playerInput, string context, EmotionalState currentEmotion) {
            if (!enableCaching) return null;

            // Clean expired entries
            CleanExpiredEntries();

            // Try the exact match first
            var cacheKey = GenerateCacheKey(playerInput, context);

            if (responseCache.TryGetValue(cacheKey, out var entry)) {
                if (IsValidCacheEntry(entry, currentEmotion)) {
                    entry.useCount++;
                    Debug.Log($"[AI Cache] Exact match found for: \"{playerInput}\"");

                    return entry.npcResponse;
                }
            }

            // Try a similarity match
            var similarEntry = FindSimilarEntry(playerInput, context, currentEmotion);

            if (similarEntry == null) {
                return null;
            }

            Debug.Log($"[AI Cache] Similar match found (score: {similarEntry.similarityScore:F2})");

            return similarEntry.npcResponse;

        }

        public void CacheResponse(string playerInput, string npcResponse, string context, EmotionalState emotion) {
            if (!enableCaching) {
                return;
            }

            var cacheKey = GenerateCacheKey(playerInput, context);

            var entry = new CacheEntry {
                playerInput = playerInput,
                npcResponse = npcResponse,
                context = context,
                timestamp = Time.time,
                useCount = 1,
                emotionalState = emotion
            };

            // Update or add an entry
            if (!responseCache.TryAdd(cacheKey, entry)) {
                responseCache[cacheKey] = entry;
            } else {
                cacheOrder.Enqueue(cacheKey);

                // Maintain cache size
                while (cacheOrder.Count > maxCacheSize) {
                    var oldestKey = cacheOrder.Dequeue();
                    responseCache.Remove(oldestKey);
                }
            }

            Debug.Log($"[AI Cache] Cached response for: \"{playerInput}\"");
        }

        private static string GenerateCacheKey(string playerInput, string context) {
            // Normalize input for better matching
            var normalizedInput = playerInput.ToLower().Trim();
            normalizedInput = Regex.Replace(normalizedInput, @"\s+", " ");

            return $"{context}::{normalizedInput}";
        }

        private bool IsValidCacheEntry(CacheEntry entry, EmotionalState currentEmotion) {
            // Check temporal consistency
            if (enforceTemporal) {
                var hoursSinceCache = (Time.time - entry.timestamp) / 3600f;

                if (hoursSinceCache > cacheExpirationHours) {
                    return false;
                }
            }

            // Check emotional consistency
            if (enforceEmotional && entry.emotionalState != currentEmotion) {
                // Allow some flexibility for neutral emotions
                if (!(entry.emotionalState == EmotionalState.Neutral ||
                      currentEmotion == EmotionalState.Neutral)) {
                    return false;
                }
            }

            return true;
        }

        private CacheEntry FindSimilarEntry(string playerInput, string context, EmotionalState currentEmotion) {
            CacheEntry bestMatch = null;
            var bestScore = 0f;

            foreach (var (key, entry) in responseCache) {
                if (!key.StartsWith(context + "::")) {
                    continue;
                }

                if (!IsValidCacheEntry(entry, currentEmotion)) {
                    continue;
                }

                var similarity = CalculateSimilarity(playerInput, entry.playerInput);

                if (similarity > similarityThreshold && similarity > bestScore) {
                    bestScore = similarity;
                    bestMatch = entry;
                    bestMatch.similarityScore = similarity;
                }
            }

            return bestMatch;
        }

        private static float CalculateSimilarity(string input1, string input2) {
            // Simple word-based similarity (in production, use better algorithms)
            var words1 = input1.ToLower().Split(' ').Where(w => w.Length > 2).ToHashSet();
            var words2 = input2.ToLower().Split(' ').Where(w => w.Length > 2).ToHashSet();

            if (words1.Count == 0 || words2.Count == 0) {
                return 0f;
            }

            var intersection = words1.Intersect(words2).Count();
            var union = words1.Union(words2).Count();

            return (float)intersection / union;
        }

        private void CleanExpiredEntries() {
            var expiredKeys = responseCache
                .Where(kvp => (Time.time - kvp.Value.timestamp) / 3600f > cacheExpirationHours)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys) {
                responseCache.Remove(key);
            }
        }

        public void PreloadCommonResponses(string npcName) {
            // Preload common interactions to ensure consistency
            var commonInteractions = new Dictionary<string, string> {
                { "hello", "Greetings, traveler." },
                { "goodbye", "Safe travels." },
                { "thank you", "You're welcome." },
                { "help", "What do you need assistance with?" }
            };

            foreach (var kvp in commonInteractions) {
                CacheResponse(kvp.Key, kvp.Value, npcName, EmotionalState.Neutral);
            }
        }

        private CacheStatistics GetStatistics() {
            return new CacheStatistics {
                totalEntries = responseCache.Count,
                totalHits = responseCache.Sum(kvp => kvp.Value.useCount),
                averageUseCount =
                    responseCache.Count > 0 ? (float)responseCache.Average(kvp => kvp.Value.useCount) : 0f,
                oldestEntry = responseCache.Count > 0 ? responseCache.Min(kvp => kvp.Value.timestamp) : 0f
            };
        }


        #if UNITY_EDITOR
        [ContextMenu("Print Cache Statistics")]
        private void PrintCacheStats() {
            var stats = GetStatistics();

            Debug.Log($"[AI Cache] Statistics:\n" +
                      $"Total Entries: {stats.totalEntries}\n" +
                      $"Total Hits: {stats.totalHits}\n" +
                      $"Average Use Count: {stats.averageUseCount:F2}\n" +
                      $"Oldest Entry Age: {(Time.time - stats.oldestEntry) / 3600f:F1} hours");
        }
        #endif

    }
}