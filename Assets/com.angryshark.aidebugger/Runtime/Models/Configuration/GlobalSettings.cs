using System;

namespace AngrySharkStudio.LLM.Models.Configuration {
    [Serializable]
    public class GlobalSettings {

        public int maxTokens = 150;
        public float temperature = 0.7f;

    }
}