using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngrySharkStudio.LLM.Models.Performance;
using UnityEngine;

namespace AngrySharkStudio.LLM.Core {
    public class AiPerformanceMonitor : MonoBehaviour {

        [Header("Monitoring Settings")]
        public bool enableMonitoring = true;
        public float metricsUpdateInterval = 5f;
        public int maxMetricsHistory = 1000;

        [Header("Performance Thresholds")]
        public float warningResponseTime = 2f;
        public float criticalResponseTime = 5f;
        public float acceptableErrorRate = 0.05f; // 5%

        [Header("Current Metrics")]
        [SerializeField] private float errorRate;
        [SerializeField] private float averageResponseTime;
        [SerializeField] private int totalRequests;
        [SerializeField] private int failedRequests;

        private readonly List<MetricEntry> metricsHistory = new();
        private float lastMetricsUpdate;


        private void Update() {
            if (!enableMonitoring) {
                return;
            }

            if (Time.time - lastMetricsUpdate > metricsUpdateInterval) {
                UpdateMetrics();
                lastMetricsUpdate = Time.time;
            }
        }

        public void RecordMetric(string npcName, float responseTime,
            bool success, string errorType = null,
            int promptTokens = 0, int responseTokens = 0) {
            var metric = new MetricEntry {
                timestamp = Time.time,
                npcName = npcName,
                responseTime = responseTime,
                success = success,
                errorType = errorType,
                promptTokens = promptTokens,
                responseTokens = responseTokens
            };

            metricsHistory.Add(metric);

            // Maintain history size
            if (metricsHistory.Count > maxMetricsHistory) {
                metricsHistory.RemoveAt(0);
            }

            totalRequests++;

            if (!success) {
                failedRequests++;
            }

            // Check for performance issues
            if (responseTime > criticalResponseTime) {
                Debug.LogError($"[AI Performance] Critical response time: {responseTime:F2}s for {npcName}");
            } else if (responseTime > warningResponseTime) {
                Debug.LogWarning($"[AI Performance] Slow response: {responseTime:F2}s for {npcName}");
            }
        }

        private void UpdateMetrics() {
            if (metricsHistory.Count == 0) return;

            var recentMetrics = metricsHistory
                .Where(m => m.timestamp > Time.time - 5 * 60) // 5 minutes ago
                .ToList();

            if (recentMetrics.Count <= 0) {
                return;
            }
            
            averageResponseTime = recentMetrics.Average(m => m.responseTime);
            errorRate = 1f - recentMetrics.Count(m => m.success) / (float)recentMetrics.Count;

            if (errorRate > acceptableErrorRate) {
                Debug.LogWarning($"[AI Performance] High error rate: {errorRate:P}");
            }
        }

        private PerformanceReport GenerateReport(float startTime, float endTime) {
            var relevantMetrics = metricsHistory
                .Where(m => m.timestamp >= startTime && m.timestamp <= endTime)
                .ToList();

            if (relevantMetrics.Count == 0) {
                return new PerformanceReport();
            }

            var report = new PerformanceReport {
                totalRequests = relevantMetrics.Count,
                successfulRequests = relevantMetrics.Count(m => m.success),
                failedRequests = relevantMetrics.Count(m => !m.success),
                averageResponseTime = relevantMetrics.Average(m => m.responseTime),
                reportGeneratedTime = Time.time,
                // Error breakdown
                errorTypeCounts = relevantMetrics
                    .Where(m => !m.success && !string.IsNullOrEmpty(m.errorType))
                    .GroupBy(m => m.errorType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                // NPC performance
                npcAverageResponseTimes = relevantMetrics
                    .GroupBy(m => m.npcName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Average(m => m.responseTime)
                    )
            };

            return report;
        }

        private void ExportMetrics(string filename = null) {
            filename ??= $"ai_metrics_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

            var path = Application.persistentDataPath + "/" + filename;

            using (var writer = new StreamWriter(path)) {
                // Write header
                writer.WriteLine("Timestamp,NPC,ResponseTime,Success,ErrorType,PromptTokens,ResponseTokens");

                // Write data
                foreach (var metric in metricsHistory) {
                    writer.WriteLine($"{metric.timestamp:F2}," +
                                     $"{metric.npcName}," +
                                     $"{metric.responseTime:F3}," +
                                     $"{metric.success}," +
                                     $"{metric.errorType ?? "None"}," +
                                     $"{metric.promptTokens}," +
                                     $"{metric.responseTokens}");
                }
            }

            Debug.Log($"[AI Performance] Metrics exported to: {path}");
        }

        private void OnApplicationPause(bool pauseStatus) {
            if (pauseStatus && enableMonitoring) {
                // Auto-save metrics when the app is paused
                ExportMetrics();
            }
        }

        #if UNITY_EDITOR
        [ContextMenu("Generate Test Report")]
        private void GenerateTestReport() {
            // Last 24 hours
            var report = GenerateReport(Time.time - 86400f, Time.time); 

            Debug.Log("Performance Report:\n" +
                      $"Total Requests: {report.totalRequests}\n" +
                      $"Avg Response Time: {report.averageResponseTime:F2}s\n" +
                      $"Successful: {report.successfulRequests}\n" +
                      $"Failed: {report.failedRequests}");
        }
        #endif

    }
}