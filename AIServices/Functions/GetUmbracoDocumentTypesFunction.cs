using AIServices.Models;
using AutoMapper;
using OpenAI.Builders;
using OpenAI.ObjectModels.RequestModels;
using System.Text;
using System.Text.Json;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class GetUmbracoDocumentTypesFunction : IUmbracoOpenAIFunction
    {
        public string Name { get => nameof(GetUmbracoDocumentTypesFunction); }

        private readonly IContentTypeService contentTypeService;
        private readonly IMapper mapper;

        public GetUmbracoDocumentTypesFunction(IContentTypeService _contentTypeService, IMapper mapper)
        {
            contentTypeService = _contentTypeService;
            this.mapper = mapper;
        }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Retrieve a comprehensive list of available Umbraco document types, including detailed information about their associated properties.")
                     .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            StringBuilder sb = new();

            sb.AppendLine("The Umbraco platform offers the following document types: ");

            sb.AppendLine(Constants.Markdown.CODEBLOCK);

            sb.AppendLine(JsonSerializer.Serialize(contentTypeService.GetAll().Select(s => mapper.Map<MinimalContentType>(s as ContentType))));

            sb.AppendLine(Constants.Markdown.CODEBLOCK);

            return sb.ToString();
        }
    }
}
