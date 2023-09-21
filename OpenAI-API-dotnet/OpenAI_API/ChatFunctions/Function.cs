using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace OpenAI_API.ChatFunctions
{
    /// <summary>
    /// Represents a Function object for the OpenAI API.
    /// A Function contains information about the function to be called, its description and parameters.
    /// </summary>
    /// <remarks>
    /// The 'Name' property represents the name of the function and must consist of alphanumeric characters, underscores, or dashes, with a maximum length of 64.
    /// The 'Description' property is an optional field that provides a brief explanation about what the function does.
    /// The 'Parameters' property describes the parameters that the function accepts, which are represented as a JSON Schema object. 
    /// Various types of input are acceptable for the 'Parameters' property, such as a JObject, a Dictionary of string and object, an anonymous object, or any other serializable object. 
    /// If the object is not a JObject, it will be converted into a JObject. 
    /// Refer to the 'Parameters' property setter for more details.
    /// Refer to the OpenAI API <see href="https://platform.openai.com/docs/guides/gpt/function-calling">guide</see> and the 
    /// JSON Schema <see href="https://json-schema.org/understanding-json-schema/">reference</see> for more details on the format of the parameters.
    /// </remarks>
    public class Function
    {
        /// <summary>
        /// The name of the function to be called. Must be a-z, A-Z, 0-9, or contain underscores and dashes, with a maximum length of 64.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }
        /// <summary>
        /// The description of what the function does.
        /// </summary>
        [JsonProperty("description", Required = Required.Default)]
        public string Description { get; set; }
        
        private JObject _parameters;
        /// <summary>
        /// The parameters that the function accepts, described as a JSON Schema object.
        /// The JSON Schema defines the type and structure of the data. It should be compatible with the JSON Schema standard.
        /// This property can accept values in various forms which can be serialized into a JSON format:
        /// 1. A JSON string, which will be parsed into a JObject.
        /// 2. A JObject, which represents a JSON object, is assigned directly.
        /// 3. A Dictionary of string and object, where keys are property names and values are their respective data.
        /// 4. An anonymous object, which gets converted into a JObject.
        /// 5. Any other object that can be serialized into a JSON format, which will be converted into a JObject.
        /// If the value cannot be converted into a JSON object, an exception will be thrown.
        /// Refer to the <see href="https://platform.openai.com/docs/guides/gpt/function-calling">guide</see> for examples and the 
        /// <see href="https://json-schema.org/understanding-json-schema/">JSON Schema reference</see> for detailed documentation about the format.
        /// </summary>
        [JsonProperty("parameters", Required = Required.Default)]
        public object Parameters
        {
            get
            {
                return _parameters;
            }
            set
            {
                try
                {
                    if (value is string jsonStringValue)
                    {
                        _parameters = JObject.Parse(jsonStringValue);
                    }
                    else if (value is JObject jObjectValue)
                    {
                        _parameters = jObjectValue;
                    }
                    else
                    {
                        var settings = new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        };
                        _parameters = JObject.FromObject(value, JsonSerializer.Create(settings));
                    }
                }
                catch (JsonException e)
                {
                    throw new ArgumentException("Could not convert the provided object into a JSON object. Make sure that the object is serializable and its structure matches the required schema.", e);
                }
            }
        }
        /// <summary>
        /// Create a function which can be applied to 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="parameters"></param>
        public Function(string name, string description, object parameters)
        {
            this.Name = name;
            this.Description = description;
            this.Parameters = parameters;
        }
        /// <summary>
        /// Creates an empty Function object.
        /// </summary>
        public Function()
        {
        }
    }
}