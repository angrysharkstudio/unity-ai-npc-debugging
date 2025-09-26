using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngrySharkStudio.LLM.API;
using AngrySharkStudio.LLM.Models.Queue;
using AngrySharkStudio.LLM.Models.Requests;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    /// <summary>
    /// Manages AI request queue to prevent overloading and ensure responsive NPCs
    /// Handles priority requests and concurrent request limits
    /// </summary>
    public class AiRequestQueue : MonoBehaviour {

        [Header("Queue Settings")]
        [Tooltip("Maximum number of API requests that can be processed simultaneously")]
        [SerializeField] private int maxConcurrentRequests = 3;

        [Tooltip("Cooldown time in seconds between processing requests")]
        [SerializeField] private float requestCooldown = 0.5f;

        [Header("Queue Status")]
        [SerializeField] private int currentRequests;
        [SerializeField] private int queueSize;

        private Queue<AiRequest> requestQueue = new();
        private bool isProcessing;
        private LlmManager llmManager;


        private void Start() {
            // Get LLMManager instance
            llmManager = LlmManager.Instance;

            if (llmManager == null) {
                Debug.LogError("[AiRequestQueue] LlmManager not found! Make sure it exists in the scene.");
            }
        }

        /// <summary>
        /// Queue an AI request with optional priority
        /// </summary>
        /// <param name="npcName">Name of the NPC making request</param>
        /// <param name="prompt">The prompt to send to AI</param>
        /// <param name="priority">Priority level (0-10, higher = more urgent)</param>
        /// <returns>The AI response when ready</returns>
        public async Task<string> QueueRequest(string npcName, string prompt, int priority = 0) {
            var request = new AiRequest {
                npcName = npcName,
                prompt = prompt,
                priority = priority,
                taskCompletion = new TaskCompletionSource<string>(),
                queueTime = Time.time
            };

            // High-priority requests go to the front
            if (priority > 5) {
                var tempQueue = new Queue<AiRequest>();
                tempQueue.Enqueue(request);

                while (requestQueue.Count > 0) {
                    tempQueue.Enqueue(requestQueue.Dequeue());
                }

                requestQueue = tempQueue;

                Debug.Log($"[AiRequestQueue] High priority request added for {npcName}");
            } else {
                requestQueue.Enqueue(request);
            }

            queueSize = requestQueue.Count;
            _ = ProcessQueue(); // Fire and forget

            return await request.taskCompletion.Task;
        }

        private async Task ProcessQueue() {
            if (isProcessing || requestQueue.Count == 0) return;

            isProcessing = true;

            while (requestQueue.Count > 0 && currentRequests < maxConcurrentRequests) {
                var request = requestQueue.Dequeue();
                currentRequests++;
                queueSize = requestQueue.Count;

                // Log queue time
                var waitTime = Time.time - request.queueTime;

                if (waitTime > 2f) {
                    Debug.LogWarning($"[AiRequestQueue] Request waited {waitTime:F2}s in queue for {request.npcName}");
                }

                _ = ProcessRequest(request); // Fire and forget with error logging

                // Add cooldown between requests
                await Task.Delay((int)(requestCooldown * 1000));
            }

            isProcessing = false;

            // Check if more requests arrived while processing
            if (requestQueue.Count > 0 && currentRequests < maxConcurrentRequests) {
                _ = ProcessQueue(); // Fire and forget
            }
        }

        private async Task ProcessRequest(AiRequest request) {
            try {
                if (llmManager == null) {
                    throw new Exception("LlmManager not initialized");
                }

                // Get response from LLMManager
                var response = await llmManager.GetAIResponse(request.prompt);

                // Complete the task with a response
                request.taskCompletion.SetResult(response);

                Debug.Log($"[AiRequestQueue] Completed request for {request.npcName}");
            } catch (Exception e) {
                Debug.LogError($"[AiRequestQueue] Error processing request for {request.npcName}: {e.Message}");

                // Return error or fallback response
                request.taskCompletion.SetException(e);
            } finally {
                currentRequests--;
                queueSize = requestQueue.Count;

                // Continue processing queue if needed
                _ = ProcessQueue(); // Fire and forget
            }
        }

        /// <summary>
        /// Get current queue status
        /// </summary>
        public QueueStatus GetQueueStatus() {
            return new QueueStatus {
                queueSize = requestQueue.Count,
                activeRequests = currentRequests,
                isProcessing = isProcessing,
                canAcceptRequests = currentRequests < maxConcurrentRequests
            };
        }


        /// <summary>
        /// Clear all pending requests (use with caution)
        /// </summary>
        public void ClearQueue() {
            while (requestQueue.Count > 0) {
                var request = requestQueue.Dequeue();
                request.taskCompletion.SetCanceled();
            }

            queueSize = 0;
            Debug.LogWarning("[AiRequestQueue] Queue cleared - all pending requests cancelled");
        }

        #if UNITY_EDITOR
        private void OnGUI() {
            // Debug display in editor
            if (Application.isEditor && isProcessing) {
                GUI.Label(new Rect(10, 10, 200, 20), $"AI Queue: {queueSize} pending, {currentRequests} active");
            }
        }
        #endif

    }
}