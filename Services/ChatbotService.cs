using System.Text.Json;

namespace CollegeChatbot.Services
{

    public class ChatBotService: IChatBotService
    {
        private readonly List<string> _collegeData;

        public ChatBotService()
        {
            _collegeData = LoadCollegeData();
        }

        private List<string> LoadCollegeData()
        {
            string filePath = "scraped_data.json";
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<List<string>>(jsonData) ?? new List<string>();
            }
            return new List<string>();
        }

        public string GetResponse(string query)
        {
            foreach (var entry in _collegeData)
            {
                if (entry.ToLower().Contains(query.ToLower()))
                    return entry;
            }
            return "Sorry, I couldn't find relevant information for that query.";
        }
    }
}
