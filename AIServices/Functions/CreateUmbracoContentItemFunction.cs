using AutoMapper;
using OpenAI.Builders;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels.SharedModels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class CreateUmbracoContentItemFunction : IUmbracoOpenAIFunction
    {
        private readonly IContentService contentService;
        private readonly IMapper mapper;

        public CreateUmbracoContentItemFunction(IContentService contentService, IMapper mapper)
        {
            this.contentService = contentService;
            this.mapper = mapper;
        }

        public string Name { get => nameof(CreateUmbracoContentItemFunction); }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Create a page (also known as a content item) of a certain document type and set the properties of the page with values.")
                    .AddParameter("content_item_name", new PropertyDefinition { Type = "string", Description = "The name of the page, e.g. Blog", Required = new List<string> { "content_item_name" } })
                    .AddParameter("content_item_document_type", new PropertyDefinition { Type = "string", Description = "The name or alias of the document type, e.g. BlogItem", Required = new List<string> { "content_item_document_type" } })
                    .AddParameter("content_item_parent_id", new PropertyDefinition { Type = "integer", Description = "The id of the parent content item"})
                    .AddParameter("content_propertie_values", PropertyDefinition.DefineArray(
                        PropertyDefinition.DefineObject(
                            properties: new Dictionary<string, PropertyDefinition>()
                            {
                                { "property_name", new PropertyDefinition { Type = "string", Description = "The name of the property"}},
                                { "property_content", new PropertyDefinition { Type = "string", Description = "The value for the property"}},
                            },
                            required: null,
                            additionalProperties: false,
                            description: "The properties to set for the page",
                            @enum: null))
                    )
                    .Build();
        }

        public string ExecuteFunction(string? arguments)
        {
            try
            {
                StringBuilder sb = new();

                ContentItem contentItem = JsonSerializer.Deserialize<ContentItem>(arguments ?? string.Empty) ?? new ContentItem();

                if(string.IsNullOrEmpty(contentItem.ContentItemName))
                    throw new ArgumentException("The name of the page is mandatory", nameof(contentItem.ContentItemName));

                if (string.IsNullOrEmpty(contentItem.ContentItemDocumentType))
                    throw new ArgumentException("The document type of the page is mandatory", nameof(contentItem.ContentItemDocumentType));

                var parentId = contentItem.ContentItemParentId <= 0 ? -1 : contentItem.ContentItemParentId;

                IContent content = contentService.Create(contentItem.ContentItemName, parentId, contentItem.ContentItemDocumentType);

                
                if (contentItem.ContentPropertiesValues?.Any() ?? false)
                {
                    foreach (var item in contentItem.ContentPropertiesValues)
                    {
                        if (content.HasProperty(item.PropertyName))
                            content.SetValue(item.PropertyName, item.PropertyContent);
                    }
                }

                contentService.SaveAndPublish(content);

                sb.AppendLine($"Page created '{contentItem.ContentItemName}' with id \"{content.Id}\" and document type \"{contentItem.ContentItemDocumentType}\"!");
                sb.AppendLine("Here is the complete newly created page object: ");
                sb.AppendLine(Constants.Markdown.CODEBLOCK);
                sb.AppendLine(JsonSerializer.Serialize(mapper.Map<Models.MinimalContentItem>(content as Umbraco.Cms.Core.Models.Content)));
                sb.AppendLine(Constants.Markdown.CODEBLOCK);

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Unfortunately something went wrong while creating the content item: {ex.Message}";

            }
        }

        #region private classes
        private class ContentProperties
        {
            [JsonPropertyName("property_name")]
            public string PropertyName { get; set; }

            [JsonPropertyName("property_content")]
            public string PropertyContent { get; set; }
        }

        private class ContentItem
        {
            [JsonPropertyName("content_item_name")]
            public string ContentItemName { get; set; }

            [JsonPropertyName("content_item_document_type")]
            public string ContentItemDocumentType { get; set; } 
            
            [JsonPropertyName("content_item_parent_id")]
            public int ContentItemParentId { get; set; }

            [JsonPropertyName("content_propertie_values")]
            public List<ContentProperties> ContentPropertiesValues { get; set; }
        }
        #endregion
    }
}
