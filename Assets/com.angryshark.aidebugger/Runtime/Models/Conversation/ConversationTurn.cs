using System;

namespace AngrySharkStudio.LLM.Models.Conversation {
    [Serializable]
    public class ConversationTurn {

        public string playerInput;
        public string npcResponse;
        public float timestamp;

    }
}