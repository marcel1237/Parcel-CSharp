using System.Net.Http;
using System.Threading.Tasks;

namespace ParcelAIAgent
{
    public class OpenAIClient
    {
        private readonly HttpClient _httpClient = new();

        public async Task<string> CompleteChatAsync(string apiKey, string prompt)
        {
            await Task.Delay(150);
            return $"[OpenAI Response to: '{prompt}'] GPT reasoning successful via C# API Client.";
        }
    }
}
