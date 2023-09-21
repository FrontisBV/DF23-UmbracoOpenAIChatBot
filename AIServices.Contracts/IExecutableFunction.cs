using Newtonsoft.Json.Linq;
using OpenAI_API.ChatFunctions;

namespace AIServices.Contracts
{
    public interface IExecutableFunction
    {
        string Name { get; }
        string Execute(string parameters);
        Function GetFunctionParameters();
    }
}