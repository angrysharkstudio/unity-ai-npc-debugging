using System;

namespace AngrySharkStudio.LLM.Models.Debug {
    [Serializable]
    public class DebugEntry {

        public string timestamp;
        public string playerInput;
        public string aiPrompt;
        public string rawResponse;
        public string finalResponse;
        public ValidationResult validation;
        public float responseTime;
        public string apiUsed;

    }
}