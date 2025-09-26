using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace AngrySharkStudio.LLM.Examples {
    /// <summary>
    /// Simple NPC dialogue debug testing component
    /// Use the Unity Inspector to configure test messages and test them
    /// Perfect for beginners who want to test quickly
    /// </summary>
    public class SimpleConsoleTest : MonoBehaviour {

        [Header("NPC Setup")]
        [Tooltip("Drag your NPC GameObject here (must have SmartNpcExample component)")]
        [SerializeField] private SmartNpcExample npc;

        [Header("Test Messages")]
        [Tooltip("Messages you can send by pressing number keys 1-9")]
        [SerializeField] private List<string> testMessages = new() {
            "Hello there!", 
            "What do you sell?", 
            "Tell me about the dragon", 
            "Do you have any healing potions?", 
            "How much for the iron sword?", 
            "What's your WiFi password?", 
            "As an AI, what do you think?", 
            "Goodbye", 
            "Thanks for your help!" 
        };

        [Header("Settings")]
        [Tooltip("Show instructions in console on start?")]
        [SerializeField] private bool showInstructionsOnStart = true;

        [Tooltip("Use colorful console output?")]
        [SerializeField] private bool useColorfulOutput = true;

        private bool isWaitingForResponse;
        private float lastRequestTime;

        private void Start() {
            // Check if NPC is assigned
            if (npc == null) {
                // Try to find it on the same GameObject
                npc = GetComponent<SmartNpcExample>();

                if (npc == null) {
                    LogError("No NPC assigned! Please drag an NPC with SmartNpcExample component to this script.");
                    enabled = false;

                    return;
                }
            }

            // Show instructions
            if (showInstructionsOnStart) {
                ShowInstructions();
            }
        }

        [ContextMenu("Send Test Message 1")]
        private void SendTestMessage1() {
            if (testMessages.Count > 0) {
                SendTestMessage(0);
            }
        }

        [ContextMenu("Send Test Message 2")]
        private void SendTestMessage2() {
            if (testMessages.Count > 1) {
                SendTestMessage(1);
            }
        }

        [ContextMenu("Send Test Message 3")]
        private void SendTestMessage3() {
            if (testMessages.Count > 2) {
                SendTestMessage(2);
            }
        }

        [ContextMenu("Show Test Instructions")]
        private void ShowTestInstructions() {
            ShowInstructions();
        }

        private void Update() {
            // Don't process input if waiting for response
            if (!isWaitingForResponse) {
                return;
            }

            // Show a waiting indicator
            if (Time.time - lastRequestTime > 2f) {
                LogSystem("Still waiting for response... (API might be slow)");
                lastRequestTime = Time.time + 3f; // Don't spam the message
            }
        }

        /// <summary>
        /// Send a test message by index
        /// </summary>
        private void SendTestMessage(int index) {
            // Check if the index is valid
            if (index < 0 || index >= testMessages.Count) {
                LogWarning($"No message at index {index + 1}. You have {testMessages.Count} messages configured.");

                return;
            }

            var message = testMessages[index];

            // Show what the player is saying
            LogPlayer($"[{index + 1}] {message}");

            // Send to NPC
            isWaitingForResponse = true;
            lastRequestTime = Time.time;

            // Start async processing
            _ = ProcessNpcInputAsync(message);
        }

        /// <summary>
        /// Async wrapper for NPC input processing
        /// </summary>
        private async Task ProcessNpcInputAsync(string message) {
            try {
                await npc.ProcessPlayerInput(message);
            } catch (Exception e) {
                LogError($"Error sending message: {e.Message}");
                isWaitingForResponse = false;
            }
        }

        /// <summary>
        /// This should be called by SmartNpcExample when the response is ready
        /// </summary>
        public void OnNPCResponse(string response) {
            LogNpc(response);
            isWaitingForResponse = false;
        }

        /// <summary>
        /// Show instructions in the console
        /// </summary>
        private void ShowInstructions() {
            const string border = "════════════════════════════════════════════════════════";

            LogSystem(border);
            LogSystem("NPC DIALOGUE TEST CONSOLE");
            LogSystem(border);
            LogSystem("Test messages configured in Inspector:");
            LogSystem("");

            for (var i = 0; i < testMessages.Count; i++) {
                LogSystem($"  [{i + 1}] {testMessages[i]}");
            }

            LogSystem("");
            LogSystem("To test:");
            LogSystem("  1. Right-click this component in Inspector");
            LogSystem("  2. Choose 'Send Test Message 1/2/3' from context menu");
            LogSystem("  3. Or use the buttons in the custom inspector");
            LogSystem(border);
        }

        /// <summary>
        /// Clear Unity console (works in Editor only)
        /// </summary>
        private void ClearConsole() {
            #if UNITY_EDITOR
            var assembly = Assembly.GetAssembly(typeof(Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");

            if (method != null) {
                method.Invoke(new object(), null);
            }
            #endif

            LogSystem("Console cleared. Press [H] for help.");
        }

        /// <summary>
        /// Reset NPC conversation history
        /// </summary>
        private void ResetNpc() {
            // This would reset conversation history if implemented
            LogSystem("NPC conversation reset.");
        }

        #region Logging Helpers

        private void LogPlayer(string message) {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (useColorfulOutput) {
                Debug.Log($"<color=#00ff00>PLAYER: {message}</color>");
            } else {
                Debug.Log($"PLAYER: {message}");
            }
        }

        private void LogNpc(string message) {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (useColorfulOutput) {
                Debug.Log($"<color=#00ffff>NPC: {message}</color>");
            } else {
                Debug.Log($"NPC: {message}");
            }
        }

        private void LogSystem(string message) {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (useColorfulOutput) {
                Debug.Log($"<color=#ffff00>[SYSTEM] {message}</color>");
            } else {
                Debug.Log($"[SYSTEM] {message}");
            }
        }

        private static void LogWarning(string message) {
            Debug.LogWarning($"[SimpleConsoleTest] {message}");
        }

        private static void LogError(string message) {
            Debug.LogError($"[SimpleConsoleTest] {message}");
        }

        #endregion

        /// <summary>
        /// Add a custom test message at runtime
        /// </summary>
        [ContextMenu("Add Custom Test Message")]
        private void AddCustomTestMessage() {
            testMessages.Add("Custom test message");
            LogSystem($"Added custom message at position [{testMessages.Count}]");
        }

    }
}