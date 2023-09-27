using AIServices.Functions.Contracts;
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
    public class UpdateUmbracoContentItemFunction : IUmbracoOpenAIFunction
    {

        private readonly IContentService contentService;
        private readonly IMapper mapper;

        public UpdateUmbracoContentItemFunction(IContentService contentService, IMapper mapper)
        {
            this.contentService = contentService;
            this.mapper = mapper;
        }

        public string Name { get => nameof(UpdateUmbracoContentItemFunction); }

        public FunctionDefinition CreateDefinition()
        {
            return new FunctionDefinitionBuilder(this.Name, "Update a page (also known as a content item) and update the properties of the document type with values.")
                    .AddParameter("content_item_id", new PropertyDefinition { Type = "integer", Description = "The name ID of the page", Required = new List<string> { "content_item_id" } })
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

                if(contentItem.ContentItemId <= 0)
                    throw new ArgumentException("The name of the page is mandatory", nameof(contentItem.ContentItemId));

                IContent content = contentService.GetById(contentItem.ContentItemId);

                if (contentItem.ContentPropertiesValues?.Any() ?? false)
                {
                    foreach (var item in contentItem.ContentPropertiesValues)
                    {
                        if (content.HasProperty(item.PropertyName))
                            content.SetValue(item.PropertyName, item.PropertyContent);
                    }
                }

                contentService.SaveAndPublish(content);

                sb.AppendLine($"Page with id '{contentItem.ContentItemId}' Updated!");
                sb.AppendLine("Here is the complete updated page object: ");
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
            [JsonPropertyName("content_item_id")]
            public int ContentItemId { get; set; }

            [JsonPropertyName("content_propertie_values")]
            public List<ContentProperties> ContentPropertiesValues { get; set; }
        }
        #endregion
    }
}
