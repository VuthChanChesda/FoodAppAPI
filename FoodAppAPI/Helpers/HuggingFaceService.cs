using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FoodAppAPI.Helpers
{
    public class HuggingFaceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HuggingFaceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["HuggingFace:ApiKey"]
                ?? throw new Exception("API key missing. Set HuggingFace:ApiKey in appsettings.json");
        }

        // --- PUBLIC METHODS ---
        public async Task<string> GetRecipeAsync(List<string> ingredients)
        {
            // The prompt now enforces the "silent correction" and "brief chef" personality
            string prompt = $"Instruction: Act as a professional chef. If ingredients are misspelled, " +
                            "silently assume the correct meaning. Provide 1 recipe based on: " +
                            $"{string.Join(", ", ingredients)}. " +
                            "Keep it as brief as possible while remaining clear. " +
                            "Format: Title, Ingredients, Steps. Use Markdown.\n\n[RESULT]";

            // Ensure the AI stays concise at the model level

            return await ExecuteAiRequestAsync(prompt);
        }

        public async Task<string> FindNearbyMarketsAsync(string location, List<string> shoppingList)
        {
            string prompt = $"You are a local shopping assistant and geography expert. Help the user find specific real-world markets near their area.\nThe user is at: '{location}'. They need: {string.Join(", ", shoppingList)}. Identify 3-4 real market chains nearby and list which items are there.";
            return await ExecuteAiRequestAsync(prompt);
        }

        // --- CORE LOGIC ---
        private async Task<string> ExecuteAiRequestAsync(string prompt)
        {
            try
            {
                // Using HuggingFaceTB/SmolLM3-3B - free on hf-inference provider
                string model = "HuggingFaceTB/SmolLM3-3B";

                // OpenAI-compatible format
                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 150,
                    temperature = 0.5
                };

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                // OpenAI-compatible endpoint
                var url = "https://router.huggingface.co/v1/chat/completions";
                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"HF API Error ({response.StatusCode}): {error}";
                }

                // OpenAI-compatible response format
                var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>();
                return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "No response.";
            }
            catch (Exception ex)
            {
                return $"System Error: {ex.Message}";
            }
        }
    }

    // --- RESPONSE DTOs for OpenAI-compatible format ---
    public class ChatCompletionResponse
    {
        [JsonPropertyName("choices")]
        public List<ChatChoice>? Choices { get; set; }
    }

    public class ChatChoice
    {
        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }
    }

    public class ChatMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }
}