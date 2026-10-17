using System.Threading.Tasks;

namespace ParcelAIAgent
{
    public class MLService
    {
        public async Task<string> RunInferenceAsync(string inputData)
        {
            await Task.Delay(50);
            return $"ML Inference Result for: {inputData} [Confidence: 99.8%] [Model: Nano-SS-v1]";
        }
    }
}
