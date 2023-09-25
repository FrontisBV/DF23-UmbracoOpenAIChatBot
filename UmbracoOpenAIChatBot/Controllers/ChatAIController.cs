using Microsoft.AspNetCore.Mvc;
using OpenAI.ObjectModels.RequestModels;
using Umbraco.Cms.Web.Common.Controllers;
using OpenAI.Interfaces;
using AIServices;

namespace InternalTools.ContextBot.Web.Controllers
{

    public class ChatAIController : UmbracoApiController
  {
        private const string defaultChatId = "default";

        private readonly IUmbracoOpenAIAppService umbracoOpenAIAppService;

        public ChatAIController(IUmbracoOpenAIAppService umbracoOpenAIAppService)
        {
            this.umbracoOpenAIAppService = umbracoOpenAIAppService;
        }

        [HttpPost]
        public void ClearMessages()
        {
            umbracoOpenAIAppService.ClearMessages(defaultChatId);
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage([FromBody] ChatMessage content)
        {
            var result = await umbracoOpenAIAppService.SendMessage(defaultChatId, content);

            return new JsonResult(new List<object>() { result.Last() });
        }
  }
}
