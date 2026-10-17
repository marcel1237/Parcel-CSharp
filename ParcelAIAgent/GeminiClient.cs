using System.Net.Http;
using System.Threading.Tasks;

namespace ParcelAIAgent
{
    public class GeminiClient
    {
        private readonly HttpClient _httpClient = new();

        public async Task<string> GenerateContentAsync(string apiKey, string prompt)
        {
            await Task.Delay(150);
            return $"[Gemini Response to: '{prompt}'] AI model execution successful via C# API Client.";
        }
    }
}
