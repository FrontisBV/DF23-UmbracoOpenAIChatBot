using OpenAI_API.Chat;

namespace AIServices.Contracts
{
    public interface IOpenAIAppService
    {
        Task<string> Chat(List<ChatMessage> messages);
    }
}