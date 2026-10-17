using System.Threading.Tasks;

namespace ParcelAIAgent
{
    public class WebSearchService
    {
        public async Task<string> SearchAsync(string query)
        {
            await Task.Delay(200);
            return $"Web Search Results for '{query}': [1] High fidelity technical synthesis from CppReference & GitHub. [2] Industrial C# .NET 8 best practices.";
        }
    }
}
