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

        public CreateUmbracoContentItemFunction(IContentService contentService)
        {
            this.contentService = contentService;
        }

        public string Name { get => nameof(CreateUmbracoContentItemFunction); }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Create a page (also known as a content item) of a certain document type and fill the properties of the document type with values.")
                    .AddParameter("content_item_name", new PropertyDefinition { Type = "string", Description = "The name of the page, e.g. Blog", Required = new List<string> { "content_item_name" } })
                    .AddParameter("content_item_document_type", new PropertyDefinition { Type = "string", Description = "The name or alias of the document type, e.g. BlogItem", Required = new List<string> { "content_item_document_type" } })
                    .AddParameter("content_propertie_values", PropertyDefinition.DefineArray(
                        PropertyDefinition.DefineObject(
                            properties: new Dictionary<string, PropertyDefinition>()
                            {
                                { "property_name", new PropertyDefinition { Type = "string", Description = "The name of the property"}},
                                { "property_content", new PropertyDefinition { Type = "string", Description = "The value for the property"}},
                            },
                            required: null,
                            additionalProperties: false,
                            description: "The properties for the specified document type which exist of the name of the property and the value for the property.",
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

                var parentId = -1;

                IContent content = contentService.Create(contentItem.ContentItemName, parentId, contentItem.ContentItemDocumentType);

                sb.AppendLine($"Page '{contentItem.ContentItemName}' with document type '{contentItem.ContentItemDocumentType}' created. Here are all the properties for the page: ");

                if (contentItem.ContentPropertiesValues?.Any() ?? false)
                {
                    foreach (var item in contentItem.ContentPropertiesValues)
                    {
                        if (content.HasProperty(item.PropertyName))
                            content.SetValue(item.PropertyName, item.PropertyContent);
                    }
                }

                contentService.SaveAndPublish(content);

                sb.AppendLine("````");
                sb.AppendLine(JsonSerializer.Serialize(content as Umbraco.Cms.Core.Models.Content));
                sb.AppendLine("````");

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

            [JsonPropertyName("content_propertie_values")]
            public List<ContentProperties> ContentPropertiesValues { get; set; }
        }
        #endregion
    }
}
