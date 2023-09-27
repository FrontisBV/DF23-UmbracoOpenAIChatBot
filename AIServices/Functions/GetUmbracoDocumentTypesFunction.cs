using AIServices.Functions.Contracts;
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
            return new FunctionDefinitionBuilder(this.Name, "Get all the document types (and their properties) that are available within umbraco")
                     .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            StringBuilder sb = new();

            try
            {
                sb.AppendLine("The following document types are available within Umbraco: ");

                sb.AppendLine(Constants.Markdown.CODEBLOCK);

                sb.AppendLine(JsonSerializer.Serialize(contentTypeService.GetAll().Select(s => mapper.Map<MinimalContentType>(s as ContentType))));

                sb.AppendLine(Constants.Markdown.CODEBLOCK);
            }
            catch (Exception ex)
            {
                sb.AppendLine($"I am sorry, i cannot provide you any document types because something went wrong. This is the internal error: {ex.Message}");
            }

            return sb.ToString();
        }
    }
}
