using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Requests {
    [Serializable]
    public class ClaudeRequest {

        public string model;
        public List<ClaudeMessage> messages;
        // ReSharper disable once InconsistentNaming
        public int max_tokens;
        public float temperature;

    }

    [Serializable]
    public class ClaudeMessage {

        public string role;
        public string content;

    }
}