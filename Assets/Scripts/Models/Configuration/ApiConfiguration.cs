using System;

namespace AngrySharkStudio.LLM.Models.Configuration {
    [Serializable]
    public class APIConfiguration {

        public string activeProvider = "openai";
        public GlobalSettings globalSettings = new();
        public ProvidersConfig providers = new();

    }
}