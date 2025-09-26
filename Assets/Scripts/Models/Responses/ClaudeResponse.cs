using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Responses {
    [Serializable]
    public class ClaudeResponse {

        public List<ClaudeContent> content;

    }

    [Serializable]
    public class ClaudeContent {

        public string text;

    }
}