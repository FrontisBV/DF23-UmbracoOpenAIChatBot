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
    public class GetUmbracoDocumentTypesFunction : IUmbracoOpenAIFunction
    {
        public string Name { get => nameof(GetUmbracoDocumentTypesFunction); }

        private readonly IContentTypeService contentTypeService;

        public GetUmbracoDocumentTypesFunction(IContentTypeService _contentTypeService)
        {
            contentTypeService = _contentTypeService;
        }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Get all the document types (and their properties) that are available within umbraco")
                     .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            StringBuilder sb = new();

            sb.AppendLine("The following document types are available within Umbraco: ");
            sb.AppendLine("````");

            sb.AppendLine(JsonSerializer.Serialize(contentTypeService.GetAll().Select(s => s as ContentType)));

            sb.AppendLine("````");

            return sb.ToString();
        }
    }
}
