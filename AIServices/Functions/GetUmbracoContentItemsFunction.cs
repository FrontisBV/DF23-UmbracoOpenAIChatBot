using AutoMapper;
using OpenAI.Builders;
using OpenAI.ObjectModels.RequestModels;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class GetUmbracoContentItemsFunction:  IUmbracoOpenAIFunction
    {
        public string Name { get => nameof(GetUmbracoContentItemsFunction); }

        private readonly IContentService contentService;
        private readonly IMapper mapper;

        public GetUmbracoContentItemsFunction(IContentService contentService, IMapper mapper)
        {
            this.contentService = contentService;
            this.mapper = mapper;
        }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Retrieve all current Umbraco webpages located at the site's root")
                     .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            StringBuilder sb = new();

            sb.AppendLine("The following webpages are already present at the root level in Umbraco: ");
            
            sb.AppendLine(Constants.Markdown.CODEBLOCK);

            var content = contentService.GetRootContent();

            var minimalresult = content.Select(s => mapper.Map<Models.MinimalContentItem>(s as Umbraco.Cms.Core.Models.Content));

            sb.AppendLine(JsonSerializer.Serialize(minimalresult));

            sb.AppendLine(Constants.Markdown.CODEBLOCK);

            return sb.ToString();
        }
    }
}
