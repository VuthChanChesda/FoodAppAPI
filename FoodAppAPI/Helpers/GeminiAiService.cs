using FoodAppAPI.Dtos;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FoodAppAPI.Helpers
{
    public class GeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        // Using gemini-1.5-flash for speed and cost-efficiency
        private const string ModelName = "gemini-1.5-flash";

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new Exception("Gemini ApiKey missing");
        }

        public async Task<string> GetRecipeAsync(List<string> ingredients)
        {
            string prompt = $"You are a professional chef. I have: {string.Join(", ", ingredients)}. " +
                            "Suggest 1 recipe. Format with a Title, Ingredients list, and Steps.";

            return await ExecuteGeminiRequestAsync(prompt);
        }

        public async Task<string> FindNearbyMarketsAsync(string location, List<string> shoppingList)
        {
            string prompt = $"You are a local geography expert. The user is currently at: '{location}'. " +
                            $"They are looking for: {string.Join(", ", shoppingList)}. " +
                            "List 3-4 real market chains nearby. " +
                            "Format each line EXACTLY as: [Market Name] - [Estimated Distance from user]. " +
                            "Do not list the items or add an introduction.";

            return await ExecuteGeminiRequestAsync(prompt);
        }

        private async Task<string> ExecuteGeminiRequestAsync(string prompt)
        {
            try
            {
                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
                }; 

                //var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={_apiKey}";
                var url = $"https://generativelanguage.googleapis.com/v1/models/gemini-2.5-flash:generateContent?key={_apiKey}";

                var response = await _httpClient.PostAsJsonAsync(url, requestBody);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"AI Error: {response.StatusCode} - {error}";
                }

                // Using dynamic parsing
                var result = await response.Content.ReadFromJsonAsync<dynamic>();

                // Safely extract the text using the Gemini response structure
                // Path: candidates -> [0] -> content -> parts -> [0] -> text
                string? content = result?.GetProperty("candidates")[0]
                                         .GetProperty("content")
                                         .GetProperty("parts")[0]
                                         .GetProperty("text")
                                         .GetString();

                return content ?? "No response generated from AI.";
            }
            catch (Exception ex)
            {
                return $"The AI service is temporarily unavailable. Error: {ex.Message}";
            }
        }
    }
}