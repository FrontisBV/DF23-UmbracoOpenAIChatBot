using OpenAI.Builders;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class GetUmbracoContentItemsFunction : IUmbracoOpenAIFunction
    {
        public string Name { get => nameof(GetUmbracoContentItemsFunction); }

        private readonly IContentService contentService;

        public GetUmbracoContentItemsFunction(IContentService contentService)
        {
            this.contentService = contentService;
        }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Get all the existing Umbraco content items that live in the root of the site")
                     .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            StringBuilder sb = new();

            sb.AppendLine("The following content items already exist at root level within Umbraco: ");
            
            sb.AppendLine("````");

            var content = contentService.GetRootContent();

            sb.AppendLine(JsonSerializer.Serialize(content.Select(s => s as Umbraco.Cms.Core.Models.Content)));

            sb.AppendLine("````");

            return sb.ToString();
        }
    }
}
