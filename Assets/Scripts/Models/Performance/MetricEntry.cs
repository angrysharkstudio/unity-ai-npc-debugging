using System;

namespace AngrySharkStudio.LLM.Models.Performance {
    [Serializable]
    public class MetricEntry {

        public string npcName;
        public float responseTime;
        public bool success;
        public string errorType;
        public float timestamp;
        public int promptTokens;
        public int responseTokens;

    }
}