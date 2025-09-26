using System;
using AngrySharkStudio.LLM.Models.Character;

namespace AngrySharkStudio.LLM.Models.Cache {
    [Serializable]
    public class CacheEntry {

        public string playerInput;
        public string npcResponse;
        public string context;
        public float timestamp;
        public int useCount;
        public float similarityScore;
        public EmotionalState emotionalState;

    }
}