using AIServices.Contracts;
using AIServices.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenAI_API;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace AIServices
{
    public class ApplicationComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            // Customising the behaviour of an Umbraco Application at 'start up'.
            // For example adding, removing, or replacing the core functionality of Umbraco or registering custom code to subscribe to events.
            builder.Services.AddSingleton<IOpenAIAppService, OpenAIAppService>();

            builder.Services.TryAddEnumerable(new[]
            {
                ServiceDescriptor.Singleton<IExecutableFunction, CreateUmbracoPageFunction>(),
                //ServiceDescriptor.Singleton<IExecutableFunction, GetCurrentWeatherFunction>()
                // Add other IExecutableFunction implementations here
            });
        }
    }
}
