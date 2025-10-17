using System;
using System.Threading.Tasks;

namespace AngrySharkStudio.LLM.Models.Requests {
    [Serializable]
    public class AiRequest {

        public string npcName;
        public string prompt;
        public int priority;
        public TaskCompletionSource<string> taskCompletion;
        public float queueTime;

    }
}