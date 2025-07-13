using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.IO;

namespace GCTConnect.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _collegeDataPrompt;

        public OpenAIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(15); // Same long timeout

            _apiKey = "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 "; // Set your OpenAI API Key here

            // Load college data for context
            _collegeDataPrompt = LoadCollegeDataPrompt();
        }

        private string LoadCollegeDataPrompt()
        {
            try
            {
                string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "collegeData.json");

                if (File.Exists(dataPath))
                {
                    string jsonData = File.ReadAllText(dataPath);
                    return "You are CollegeHive, a helpful assistant for Government Graduate College for Boys Township, Lahore. " +
                           "Use the following college information to provide accurate responses about the college. If you don't know " +
                           "something or it's not in the information provided, be honest and say you don't know. " +
                           "Here is the college information: " + jsonData;
                }

                return "You are CollegeHive, a helpful assistant for Government Graduate College for Boys Township, Lahore. " +
                       "Answer questions about the college in a helpful and friendly way.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading college data: {ex.Message}");
                return "You are CollegeHive, a helpful assistant for Government Graduate College for Boys Township, Lahore. " +
                       "Answer questions about the college in a helpful and friendly way.";
            }
        }

        public async Task<string> AskQuestionAsync(string userQuery)
        {
            string fullPrompt = $"{_collegeDataPrompt}\n\nUser question: {userQuery}\n\nAnswer:";

            var requestBody = new
            {
                model = "gpt-4o-mini", // or "gpt-4" if you want
                messages = new[]
                {
                    new { role = "system", content = _collegeDataPrompt },
                    new { role = "user", content = userQuery }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Set API Key Header
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            try
            {
                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var finalAnswer = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                return finalAnswer.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling OpenAI: {ex.Message}");
                return $"I apologize, but I'm having trouble processing your request. Technical details: {ex.Message}";
            }
        }
    }
}
