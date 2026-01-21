using FoodAppAPI.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FoodAppAPI.Helpers
{
    public class OpenRouterService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        private readonly string[] _modelFallbacks = new[]
        {
            "meta-llama/llama-3.1-8b-instruct:free",   
            "mistralai/mistral-7b-instruct:free",      
            "qwen/qwen-2-7b-instruct:free",             
        };

        public OpenRouterService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OpenRouter:ApiKey"] ?? throw new Exception("ApiKey missing");
        }

        // --- PUBLIC METHODS ---

        public async Task<string> GetRecipeAsync(List<string> ingredients)
        {
            const string systemMessage = "You are a professional chef. Suggest 1 recipe based on the provided ingredients. Format with a Title, Ingredients list, and Steps.";
            string userMessage = $"I have: {string.Join(", ", ingredients)}";

            return await ExecuteAiRequestAsync(systemMessage, userMessage);
        }

        public async Task<string> FindNearbyMarketsAsync(string location, List<string> shoppingList)
        {
            const string systemMessage = "You are a local shopping assistant and geography expert. Help the user find specific real-world markets near their area.";
            string userMessage = $"The user is at: '{location}'. They need: {string.Join(", ", shoppingList)}. Identify 3-4 real market chains nearby and list which items are there.";

            return await ExecuteAiRequestAsync(systemMessage, userMessage);
        }

        // --- REUSABLE CORE LOGIC ---

        private async Task<string> ExecuteAiRequestAsync(string systemMessage, string userMessage)
        {
            string lastError = "Unknown error";

            foreach (var model in _modelFallbacks)
            {
                try
                {
                    var requestBody = new
                    {
                        model = model,
                        messages = new[]
                        {
                            new { role = "system", content = systemMessage },
                            new { role = "user", content = userMessage }
                        }
                    };

                    _httpClient.DefaultRequestHeaders.Clear();
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                    _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                    _httpClient.DefaultRequestHeaders.Add("X-Title", "SmartPantry App");

                    var response = await _httpClient.PostAsJsonAsync("https://openrouter.ai/api/v1/chat/completions", requestBody);

                    if (!response.IsSuccessStatusCode)
                    {
                        lastError = await response.Content.ReadAsStringAsync();
                        continue; // Try next model
                    }

                    var result = await response.Content.ReadFromJsonAsync<OpenRouterResponse>();
                    var content = result?.Choices?[0]?.Message?.Content;

                    if (!string.IsNullOrEmpty(content)) return content;
                }
                catch (Exception ex)
                {
                    lastError = ex.Message;
                    continue;
                }
            }

            return $"The AI service is temporarily unavailable. (Last Error: {lastError})";
        }
    }
}