using Microsoft.Extensions.DependencyInjection;
using OpenAI.Extensions;
using OpenAI;
using Umbraco.Cms.Core.DependencyInjection;
using System.Reflection;
using AutoMapper;
using AIServices.Mappings;
using AIServices.ChatMessages;
using AIServices.Functions.Contracts;

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
                opt.DefaultModelId = OpenAI.ObjectModels.Models.Gpt_4_1106_preview;
            });
            #endregion

            builder.Services.AddSingleton<IChatMessagesStorageAppService, ChatMessagesStorageAppService>();
            builder.Services.AddTransient<IUmbracoOpenAIAppService, UmbracoOpenAIAppService>();

            RegisterFunctions(builder);

            SetupAutoMapper(builder);

            return builder;
        }

        private static void SetupAutoMapper(IUmbracoBuilder builder)
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<UmbacoContentMappings>();
            });

            IMapper mapper = mapperConfig.CreateMapper();
            builder.Services.AddSingleton(mapper);
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
