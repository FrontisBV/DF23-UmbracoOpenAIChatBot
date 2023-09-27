using OpenAI.ObjectModels.RequestModels;

namespace AIServices.ChatMessages
{
    public interface IChatMessagesStorageAppService
    {
        List<ChatMessage> Get(string chatId);

        void Save(string chatId, List<ChatMessage> chatMessages);

        void Clear(string chatId);
    }
}
