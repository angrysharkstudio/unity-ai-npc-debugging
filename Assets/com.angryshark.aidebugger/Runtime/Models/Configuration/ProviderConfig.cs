using System;

namespace AngrySharkStudio.LLM.Models.Configuration {
    [Serializable]
    public class ProviderConfig {

        public string apiKey = "";
        public string apiUrl = "";
        public string model = "";
        public string apiVersion = "";

    }
}