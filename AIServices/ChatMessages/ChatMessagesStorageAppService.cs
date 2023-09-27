using OpenAI.ObjectModels.RequestModels;

namespace AIServices.ChatMessages
{
    public class ChatMessagesStorageAppService : IChatMessagesStorageAppService
    {
        public Dictionary<string, List<ChatMessage>> Messages { get; private set; }

        public ChatMessagesStorageAppService()
        {
            Messages = new();
        }

        public void Clear(string chatId)
        {
            if (Messages.ContainsKey(chatId))
                Messages[chatId].Clear();
        }

        public List<ChatMessage> Get(string chatId)
        {
            if (Messages.ContainsKey(chatId))
                return Messages[chatId];

            return null;
        }

        public void Save(string chatId, List<ChatMessage> chatMessages)
        {
            Messages[chatId] = chatMessages;
        }
    }
}
