using System;

namespace AngrySharkStudio.LLM.Models.Configuration {
    [Serializable]
    public class ProvidersConfig {

        public ProviderConfig openai = new();
        public ProviderConfig claude = new();
        public ProviderConfig gemini = new();

    }
}