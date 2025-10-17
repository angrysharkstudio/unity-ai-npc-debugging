using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Requests {
    [Serializable]
    public class OpenAiRequest {

        public string model;
        public List<OpenAiMessage> messages;
        // ReSharper disable once InconsistentNaming
        public int max_tokens;
        public float temperature;

    }

    [Serializable]
    public class OpenAiMessage {

        public string role;
        public string content;

    }
}