using AIServices.Functions;
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
        private readonly IChatMessagePersistencyAppService chatMessagePersistencyAppService;
        private readonly IEnumerable<IUmbracoOpenAIFunction> openAIFunctions;
        private readonly List<FunctionDefinition> functionDefinitions;

       

        public UmbracoOpenAIAppService(
            IOpenAIService openAiAppService,
            IChatMessagePersistencyAppService chatMessagePersistencyAppService,
            IEnumerable<IUmbracoOpenAIFunction> openAIFunctions
            )
        {
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
                MaxTokens = 200,
            });

            if (completionResult.Successful)
            {
                var choice = completionResult.Choices.First();

                if (choice.Message.FunctionCall != null)
                {
                    await HandleFunctionMessage(completionResult, messages);

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

        private async Task HandleFunctionMessage(ChatCompletionCreateResponse completionResult, List<ChatMessage> messages)
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
                        await HandleFunctionMessage(newCompletionResult, messages);
                }
                else
                {
                    messages.Add(ChatMessage.FromFunction($"An error occured: {newCompletionResult.Error?.Message ?? "Unknown"}", choice.Message.FunctionCall.Name));
                }
            }

            //return messages;
        }

        private string GetFunctionResult(FunctionCall fn)
        {
            return openAIFunctions.FirstOrDefault(s => s.Name == fn.Name)?.ExecuteFunction(fn.Arguments) ?? string.Empty;
        }

        private List<ChatMessage> InitMessages(string chatId)
        {
            var messages = chatMessagePersistencyAppService.Get(chatId) ?? new ();

            if (!messages?.Any() ?? false)
            {
                messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("Act as a Umbraco bot integrated in the backoffice, developed by digital agency Frontis. Help the user."),
                    //ChatMessage.FromSystem(@"When prompted to create a page (also known as a content item) make sure you know what document types are available in Umbraco 
                    //                        (you can use the 'get_umbraco_documenttypes' function for this) 
                    //                        then make sure you execute the document type properties function so you know which properties belong to the certain document type."),

                     ChatMessage.FromSystem(@"When prompted to create a page use the document type you think suits best. You can always request Umbraco if you do not know which document types are available. 
                                               Also request the properties of the document types if needed. Also, when requested to create a page always check if the page already exists, if the page already exists do NOT create it."),

                };
            }

            return messages;
        }
    }
}
