using AIServices.Functions.Contracts;
using AIServices.ChatMessages;
using OpenAI.Builders;
using OpenAI.Interfaces;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.ResponseModels;
using OpenAI.ObjectModels.SharedModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace AIServices
{
    public class UmbracoOpenAIAppService: IUmbracoOpenAIAppService
    {
        const int MAXSYSTEMTOKENS = 4000;
        const float MODELTEMPERATURE = 0.1f;

        private readonly IOpenAIService openAiAppService;
        private readonly IChatMessagesStorageAppService chatMessagePersistencyAppService;
        private readonly IEnumerable<IUmbracoOpenAIFunction> openAIFunctions;
        private readonly List<FunctionDefinition> functionDefinitions;

        public UmbracoOpenAIAppService(
            IOpenAIService openAiAppService,
            IChatMessagesStorageAppService chatMessagePersistencyAppService,
            IEnumerable<IUmbracoOpenAIFunction> openAIFunctions
            )
        {
            this.openAiAppService = openAiAppService;
            this.chatMessagePersistencyAppService = chatMessagePersistencyAppService;
            this.openAIFunctions = openAIFunctions;

            this.functionDefinitions = openAIFunctions.Select(s => s.CreateDefinition()).ToList();
        }

        public async Task<List<ChatMessage>> SendMessage(string chatId, ChatMessage content)
        {
            var messages = InitSystemMessages(chatId);

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

        private List<ChatMessage> InitSystemMessages(string chatId)
        {
            var messages = chatMessagePersistencyAppService.Get(chatId) ?? new ();

            if (!messages?.Any() ?? false)
            {
                messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("Act as a Umbraco bot integrated in the backoffice, developed by digital agency Frontis. Help the user."),
                    ChatMessage.FromSystem("When prompted to create a page act as a content marketeer whom always want the page to be optimized for SEO. Before creating a page always request umbraco which document types are availble. The docment types will tell you all the metadata information e.q. property information, permission information, template information. Optionally you can also request what content items are already created at root level. If the document type has SEO properties then fill them.")
                };
            }

            return messages;
        }

        public void ClearMessages(string chatId)
        {
            chatMessagePersistencyAppService.Clear(chatId);
        }
    }
}
