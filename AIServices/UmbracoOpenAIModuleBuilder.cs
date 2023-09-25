using AIServices.Functions;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Extensions;
using OpenAI;
using Umbraco.Cms.Core.DependencyInjection;
using OpenAI.ObjectModels;
using System.Reflection;
using Umbraco.Cms.Core.Services;

namespace AIServices
{
    public static class UmbracoOpenAIModuleBuilder
    {
        public static IUmbracoBuilder AddOpenAIContextBot(this IUmbracoBuilder builder)
        {
            var configuration = builder.Config;

            #region openAI
            builder.Services.Configure<OpenAiOptions>(configuration.GetSection("OpenAIServiceOptions"));

            builder.Services.AddOpenAIService(opt =>
            {
                opt.DefaultModelId = Models.Gpt_3_5_Turbo_16k;
            });
            #endregion

            builder.Services.AddSingleton<IChatMessagePersistencyAppService, ChatMessageSessionPersistencyAppService>();
            builder.Services.AddTransient<IUmbracoOpenAIAppService, UmbracoOpenAIAppService>();

            RegisterFunctions(builder);

            return builder;
        }

        private static void RegisterFunctions(IUmbracoBuilder builder)
        {
            var functions = Assembly.GetExecutingAssembly().GetTypes()
             .Where(x => !x.IsAbstract && x.IsClass && typeof(IUmbracoOpenAIFunction).IsAssignableFrom(x));

            foreach (var function in functions)
            {
                builder.Services.Add(new ServiceDescriptor(typeof(IUmbracoOpenAIFunction), function, ServiceLifetime.Transient));
            }
        }
    }
}
