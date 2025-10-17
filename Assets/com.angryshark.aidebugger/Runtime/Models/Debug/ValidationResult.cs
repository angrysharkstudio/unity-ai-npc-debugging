using System;
using System.Collections.Generic;

namespace AngrySharkStudio.LLM.Models.Debug {
    [Serializable]
    public class ValidationResult {

        public bool passed;
        public List<string> failures = new();
        public float consistencyScore;
        public bool wasFiltered;

    }
}