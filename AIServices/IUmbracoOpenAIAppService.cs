using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIServices
{
    public interface IUmbracoOpenAIAppService
    {
        Task<List<ChatMessage>> SendMessage(string chatId, ChatMessage content);

        void ClearMessages(string chatId);
    }
}
