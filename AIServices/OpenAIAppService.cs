using AIServices.Contracts;
using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using System.Runtime;

namespace AIServices
{
    public class OpenAIAppService : IOpenAIAppService
    {
        private readonly OpenAIAPI openAIApi;
        private readonly List<IExecutableFunction> functions;

        public OpenAIAppService(IEnumerable<IExecutableFunction> functions, IOptions<OpenAISettings> settings)
        {
            this.openAIApi = new OpenAIAPI(new APIAuthentication(settings.Value.ApiKey));
            this.functions = functions.ToList();
        }

        public async Task<string> Chat(List<ChatMessage> messages)
        {
            var functionParameters = functions.Select(func => func.GetFunctionParameters()).ToList();

            var response = await openAIApi.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo0613,
                Temperature = 0.1,
                MaxTokens = 400,
                Messages = messages,
                Functions = functionParameters
            });

            int maxInteractions = 100;
            for (int i = 0; i < maxInteractions; i++)
            {
                var lastResult = response.Choices.Last();

                switch (lastResult.FinishReason)
                {
                    case "function_call":
                        response = await CallFunction(lastResult, messages);
                        break;
                    case "stop":
                    default:
                        return response.ToString();
                }
            };

            return response.ToString();
        }

        private async Task<ChatResult> CallFunction(ChatChoice lastResult, List<ChatMessage> messages)
        {
            messages.Add(new ChatMessage
            {
                Role = ChatMessageRole.Assistant,
                Content = "(null)",
                Function_Call = lastResult.Message.Function_Call
            });

            // execute function
            var function = functions.First(func => func.Name == lastResult.Message.Function_Call.Name);
            var result = function.Execute(lastResult.Message.Function_Call.Arguments);

            // Add function result back to the chat
            messages.Add(new ChatMessage
            {
                Role = ChatMessageRole.Function,
                Name = lastResult.Message.Function_Call.Name,
                Content = result
            });

            // send function result to ai
            return await openAIApi.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo0613,
                Temperature = 0.1,
                MaxTokens = 200,
                Messages = messages,
                Functions = functions.Select(func => func.GetFunctionParameters()).ToList()
            });
        }

    }
}