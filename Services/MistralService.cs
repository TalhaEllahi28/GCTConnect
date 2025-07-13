
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.IO;

namespace GCTConnect.Services
{
    public class MistralService
    {
        private readonly HttpClient _httpClient;
        private readonly string _collegeDataPrompt;

        public MistralService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromMinutes(15); // Increase timeout

            // Load the college data to provide context for the AI
            _collegeDataPrompt = LoadCollegeDataPrompt();
        }

        private string LoadCollegeDataPrompt()
        {
            try
            {
                // Path to your college data file - adjust as necessary
                string dataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "collegeData.json");

                if (File.Exists(dataPath))
                {
                    string jsonData = File.ReadAllText(dataPath);
                    return "You are CollegeHive, a helpful assistant for Government Graduate College for Boys Township, Lahore. " +
                           "Use the following college information to provide accurate responses about the college. If you don't know " +
                           "something or it's not in the information provided, be honest and say you don't know. " +
                           "Here is the college information: " + jsonData;
                }

                // Fallback if file doesn't exist
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
            // Combine college context with user query
            string fullPrompt = $"{_collegeDataPrompt}\n\nUser question: {userQuery}\n\nAnswer:";

            var request = new
            {
                model = "mistral",
                prompt = fullPrompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var finalAnswer = doc.RootElement.GetProperty("response").GetString();

                return finalAnswer;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Mistral AI: {ex.Message}");
                return $"I apologize, but I'm having trouble processing your request. Technical details: {ex.Message}";
            }
        }
    }
}
