using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Filtering {
    [Serializable]
    public class FilterResult {

        public bool passed;
        public bool wasModified;
        public string filteredContent;
        public List<string> triggeredFilters = new();
        public float confidenceScore;

    }
}