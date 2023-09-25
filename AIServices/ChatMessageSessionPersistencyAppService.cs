using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIServices
{
    public class ChatMessageSessionPersistencyAppService : IChatMessagePersistencyAppService
    {
        public Dictionary<string, List<ChatMessage>> Messages { get; private set; }

        public ChatMessageSessionPersistencyAppService()
        {
            this.Messages = new();
        }

        public void Clear(string chatId)
        {
            if (Messages.ContainsKey(chatId))
                Messages[chatId].Clear();
        }

        public List<ChatMessage> Get(string chatId)
        {
            if(Messages.ContainsKey(chatId))
                return Messages[chatId];

            return null;
        }

        public void Save(string chatId, List<ChatMessage> chatMessages)
        {
            Messages[chatId] = chatMessages;
        }
    }
}
