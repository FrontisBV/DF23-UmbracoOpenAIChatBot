using AIServices.Contracts;
using Newtonsoft.Json.Linq;
using OpenAI_API.ChatFunctions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace AIServices.Functions
{
    public class CreateUmbracoPageFunction : IExecutableFunction
    {
        private readonly IContentService _contentService;

        public string Name => "create_umbraco_page";

        public CreateUmbracoPageFunction(IContentService contentService)
        {
            _contentService = contentService;
        }

        public string Execute(string parametersString)
        {
            JObject parameters = JObject.Parse(parametersString);

            // Extract parameters from the JObject
            var name = parameters["name"].ToString();
            string description = parameters.ContainsKey("description")
                ? parameters["description"].ToString()
                : string.Empty;

            // Get the ID of the parent node to create this content under (assuming root node for simplicity)
            var parentId = -1;

            // You'll need to know the alias of the DocumentType
            var documentTypeAlias = "Content";

            // Create new content
            IContent content = _contentService.Create(name, parentId, documentTypeAlias);

            //content.SetCultureName(name, "nl-nl");

            // Set properties

            // Save the new content
            _contentService.SaveAndPublish(content);

            return $"New page '{name}' created with id {content.Id}.";
        }

        public Function GetFunctionParameters()
        {
            var parameters = new JObject
            {
                ["type"] = "object",
                ["required"] = new JArray("location"),
                ["properties"] = new JObject
                {
                    ["name"] = new JObject
                    {
                        ["type"] = "string",
                        ["description"] = "name of the page, e.g. Home"
                    },
                    ["description"] = new JObject
                    {
                        ["type"] = "string",
                        ["description"] = "description of the page"
                    }
                }
            };

            return new Function(name: "create_umbraco_page", description: "Create a new page in Umbraco", parameters);
        }
    }
}
