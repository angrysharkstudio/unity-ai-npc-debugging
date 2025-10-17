using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Requests {
    [Serializable]
    public class GeminiRequest {

        public List<GeminiContent> contents;
        public GeminiConfig generationConfig;

    }

    [Serializable]
    public class GeminiContent {

        public List<GeminiPart> parts;

    }

    [Serializable]
    public class GeminiPart {

        public string text;

    }

    [Serializable]
    public class GeminiConfig {

        public float temperature;
        public int maxOutputTokens;

    }
}