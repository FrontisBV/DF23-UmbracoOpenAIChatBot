using AIServices.Models;
using AutoMapper;
using OpenAI.Builders;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.SharedModels;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class GetUmbracoContentByIdFunction: IUmbracoOpenAIFunction
    {
        public string Name { get => nameof(GetUmbracoContentByIdFunction); }

        private readonly IContentService contentService;
        private readonly IMapper mapper;

        public GetUmbracoContentByIdFunction(IContentService contentService, IMapper mapper)
        {
            this.contentService = contentService;
            this.mapper = mapper;
        }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Retrieve a specific Umbraco webpage by its unique ID")
                  .AddParameter("content_item_id", new PropertyDefinition { Type = "integer", Description = "The unique ID of the webpage", Required = new List<string> { "content_item_id" } })
                  .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            Arguments args = JsonSerializer.Deserialize<Arguments>(arguments ?? string.Empty) ?? new Arguments();

            StringBuilder sb = new();

            var item = contentService.GetById(args.content_item_id);

            if(item == null)
            {
                sb.AppendLine($"No page found with the id \"{args.content_item_id}\"");
            }
            else
            {
                sb.AppendLine($"Here is the specific Umbraco webpage (with unique ID \"{args.content_item_id}\") you requested: ");
                sb.AppendLine(Constants.Markdown.CODEBLOCK);
                sb.AppendLine(JsonSerializer.Serialize(mapper.Map<MinimalContentItem>(item as Umbraco.Cms.Core.Models.Content)));
                sb.AppendLine(Constants.Markdown.CODEBLOCK);
                sb.AppendLine("How can I assist you further with this content item or its properties?");
            }
            
            return sb.ToString();
        }

        private class Arguments
        {
            public int content_item_id { get; set; }
        }
    }
}
