using AIServices.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Umbraco.Cms.Web.Common.Controllers;

namespace InternalTools.ContextBot.Web.Controllers
{
  public class ChatMessage
  {
    public string? User { get; set; }
    public string? Content { get; set; }
  }

  public class ChatAIController : UmbracoApiController
  {
    private readonly IOpenAIAppService _openAiAppService;

    public ChatAIController(IOpenAIAppService openAiAppService)
    {   
        _openAiAppService = openAiAppService;
    }

    [HttpPost]
    public void ClearMessages([FromBody] ChatMessage content)
    {
      // Clear the messages.
    }

    [HttpPost]
    public async Task<JsonResult> SendMessage([FromBody] ChatMessage content)
    {
      // Verwerk het ontvangen bericht, bijvoorbeeld door het naar Azure AI te sturen
      // en de respons op te halen.
      var ChatbotAnswer = await _openAiAppService.Chat(new List<OpenAI_API.Chat.ChatMessage>()
      {
        new OpenAI_API.Chat.ChatMessage()
        {
            Content = "Act as a Umbraco bot integrated in the backoffice, developed by digital agency Frontis. Help the user.",
            Role = OpenAI_API.Chat.ChatMessageRole.System
        },
        new OpenAI_API.Chat.ChatMessage()
        {
            Content = content.Content,
            Role = OpenAI_API.Chat.ChatMessageRole.User
        }
      });

      // Simuleer een ontvangen bericht van Azure AI (vervang dit door je eigen logica)
      var receivedMessage = new { Type = "received", Content = ChatbotAnswer };

      var response = new List<object> { receivedMessage };

      return new JsonResult(response);
    }
  }
}
