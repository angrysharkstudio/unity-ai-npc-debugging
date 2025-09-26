using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Performance {
    [Serializable]
    public class PerformanceReport {

        public int totalRequests;
        public int successfulRequests;
        public int failedRequests;
        public float averageResponseTime;
        public Dictionary<string, int> errorTypeCounts = new();
        public Dictionary<string, float> npcAverageResponseTimes = new();
        public float reportGeneratedTime;

    }
}