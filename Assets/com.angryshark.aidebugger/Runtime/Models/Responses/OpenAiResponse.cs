using System;
using System.Collections.Generic;
using AngrySharkStudio.LLM.Models.Requests;

namespace AngrySharkStudio.LLM.Models.Responses {
    [Serializable]
    public class OpenAiResponse {

        public List<OpenAiChoice> choices;

    }

    [Serializable]
    public class OpenAiChoice {

        public OpenAiMessage message;

    }
}