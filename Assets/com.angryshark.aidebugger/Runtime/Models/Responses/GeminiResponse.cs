using System;
using System.Collections.Generic;
using AngrySharkStudio.LLM.Models.Requests;

namespace AngrySharkStudio.LLM.Models.Responses {
    [Serializable]
    public class GeminiResponse {

        public List<GeminiCandidate> candidates;

    }

    [Serializable]
    public class GeminiCandidate {

        public GeminiContent content;

    }
}