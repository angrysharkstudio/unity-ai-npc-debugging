using System;

namespace AngrySharkStudio.LLM.Models.Queue {
    [Serializable]
    public class QueueStatus {

        public int queueSize;
        public int activeRequests;
        public bool isProcessing;
        public bool canAcceptRequests;

    }
}