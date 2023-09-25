using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIServices
{
    public interface IChatMessagePersistencyAppService
    {
        List<ChatMessage> Get(string chatId);

        void Save(string chatId, List<ChatMessage> chatMessages);

        void Clear(string chatId);
    }
}
