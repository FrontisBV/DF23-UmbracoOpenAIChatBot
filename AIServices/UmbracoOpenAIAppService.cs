using AIServices.Functions;
using AIServices.Models;
using Microsoft.Extensions.Options;
using OpenAI.Builders;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using OpenAI.ObjectModels.SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Metadata;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using static Umbraco.Cms.Core.PropertyEditors.ListViewConfiguration;

namespace AIServices
{
    public class UmbracoOpenAIAppService: IUmbracoOpenAIAppService
    {
        const int MAXSYSTEMTOKENS = 4000;
        const float MODELTEMPERATURE = 0.1f;
        private readonly UmbracoAIOptions options;
        private readonly IOpenAIService openAiAppService;
        private readonly IChatMessagePersistencyAppService chatMessagePersistencyAppService;
        private readonly IEnumerable<IUmbracoOpenAIFunction> openAIFunctions;
        private readonly List<FunctionDefinition> functionDefinitions;

        public UmbracoOpenAIAppService(
            IOptions<UmbracoAIOptions> options,
            IOpenAIService openAiAppService,
            IChatMessagePersistencyAppService chatMessagePersistencyAppService,
            IEnumerable<IUmbracoOpenAIFunction> openAIFunctions
            )
        {
            this.options = options?.Value ?? new();

            this.openAiAppService = openAiAppService;
            this.chatMessagePersistencyAppService = chatMessagePersistencyAppService;
            this.openAIFunctions = openAIFunctions;

            this.functionDefinitions = openAIFunctions.Select(s => s.CreateDefinition()).ToList();
        }

        public void ClearMessages(string chatId)
        {
            chatMessagePersistencyAppService.Clear(chatId);
        }

        public async Task<List<ChatMessage>> SendMessage(string chatId, ChatMessage content)
        {
            var messages = InitMessages(chatId);

            messages.Add(ChatMessage.FromUser(content.Content));

            var completionResult = await openAiAppService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messages,
                Functions = functionDefinitions,
                Temperature = MODELTEMPERATURE,
                MaxTokens = 1000,
            });

            if (completionResult.Successful)
            {
                var choice = completionResult.Choices.First();

                if (choice.Message.FunctionCall != null)
                {
                    await HandleFunctionMessageRecursive(completionResult, messages);

                    completionResult = await openAiAppService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                    {
                        Messages = messages,
                        Functions = functionDefinitions,
                        Temperature = 0.1f,
                        MaxTokens = MAXSYSTEMTOKENS,
                    });

                    if (completionResult.Successful)
                    {
                        choice = completionResult.Choices.First();

                        messages.Add(choice.Message);
                    }
                    else
                        messages.Add(ChatMessage.FromAssistant(completionResult.Error?.Message ?? "An error occured"));
                    
                }
                else
                {
                    messages.Add(choice.Message);
                }
            }
            else
            {
                messages.Add(ChatMessage.FromAssistant($"An error occured: {completionResult.Error?.Message ?? "Unknown"}"));
            }

            chatMessagePersistencyAppService.Save(chatId, messages);

            return messages;
        }

        private async Task HandleFunctionMessageRecursive(ChatCompletionCreateResponse completionResult, List<ChatMessage> messages)
        {
            var choice = completionResult.Choices.First();

            if(completionResult.Successful && choice.Message.FunctionCall != null)
            {
                var functionContentResult = GetFunctionResult(choice.Message.FunctionCall);

                messages.Add(ChatMessage.FromFunction(functionContentResult, choice.Message.FunctionCall.Name));

                var newCompletionResult = await openAiAppService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                {
                    Messages = messages,
                    Functions = functionDefinitions,
                    MaxTokens = MAXSYSTEMTOKENS,
                    Temperature = MODELTEMPERATURE,
                });

                if (newCompletionResult.Successful)
                {
                    var newChoice = newCompletionResult.Choices.First();

                    if (newChoice.Message.FunctionCall != null)
                        await HandleFunctionMessageRecursive(newCompletionResult, messages);
                }
                else
                {
                    messages.Add(ChatMessage.FromFunction($"An error occured: {newCompletionResult.Error?.Message ?? "Unknown"}", choice.Message.FunctionCall.Name));
                }
            }
        }

        private string GetFunctionResult(FunctionCall fn)
        {
            return openAIFunctions.FirstOrDefault(s => s.Name == fn.Name)?.ExecuteFunction(fn.Arguments) ?? string.Empty;
        }

        private List<ChatMessage> InitMessages(string chatId)
        {
            StringBuilder sb = new StringBuilder();

            var messages = chatMessagePersistencyAppService.Get(chatId) ?? new ();

            if (!messages?.Any() ?? false)
            {
                sb.AppendLine("As an OpenAI bot seamlessly integrated into the Umbraco backoffice, I'm here to assist you with creating, updating, and retrieving content pages and document types. Please let me know how I can help you with these tasks.");
                sb.AppendLine("When tasked with creating a webpage take care of the following: ");
                sb.AppendLine("- Always begin by inquiring about the available document types in Umbraco. These document types contain essential metadata, including property details, permissions, and template information.");
                sb.AppendLine("- Try to find a matching document type for the webpage that you are creating.");
                sb.AppendLine("- If desired, you can also request a list of content items at the root level.");
                sb.AppendLine("- If the selected document type includes SEO properties, ensure they are meticulously filled to enhance search engine optimization.");
                
                if (!String.IsNullOrEmpty(options.DefaultDocumentType))
                    sb.AppendLine($"- If you can not find a matchting document type then use the \"{options.DefaultDocumentType}\" as default document type");

                messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(sb.ToString())
                };
            }

            return messages;
        }
    }
}
