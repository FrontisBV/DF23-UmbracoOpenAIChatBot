using System;
using Newtonsoft.Json;

namespace OpenAI_API.ChatFunctions
{
    /// <summary>
    /// An optional class to be used with models that support returning function calls.
    /// </summary>
    public class Function_Call
    {
        /// <summary>
        /// The name of the function.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Any arguments that need to be passed to the function. This needs to be in JSON format.
        /// </summary>
        [JsonProperty("arguments")]
        public string Arguments { get; set; }
    }
}

