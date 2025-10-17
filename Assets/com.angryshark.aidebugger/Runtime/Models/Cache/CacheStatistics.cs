using System;

namespace AngrySharkStudio.LLM.Models.Cache {
    [Serializable]
    public class CacheStatistics {

        public int totalEntries;
        public int totalHits;
        public float averageUseCount;
        public float oldestEntry;

    }
}