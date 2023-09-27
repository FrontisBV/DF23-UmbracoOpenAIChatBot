using OpenAI.ObjectModels.RequestModels;

namespace AIServices.Functions.Contracts
{
    public interface IUmbracoOpenAIFunction
    {
        string Name { get; }

        FunctionDefinition CreateDefinition();

        string ExecuteFunction(string? arguments);
    }
}
