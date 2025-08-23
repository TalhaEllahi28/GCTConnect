using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System.Text.Json;

public class GeminiChatService
{
    private readonly string _apiKey = "YOUR_API_KEY";
    private readonly string _endpoint =
        "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key=";
    private readonly IWebHostEnvironment _env;

    public GeminiChatService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> GetResponse(string userQuery)
    {
        // Load dataset from wwwroot
        var dataPath = Path.Combine(_env.WebRootPath, "collegeData.json");
        var collegeData = await File.ReadAllTextAsync(dataPath);

        // Combine dataset + query
        var prompt = $"College Data: {collegeData}\n\nUser Question: {userQuery}\n\nAnswer in a clear and concise way.";

        // Send to Gemini
        var client = new RestClient(_endpoint + _apiKey);
        var request = new RestRequest("", RestSharp.Method.Post);
        request.AddHeader("Content-Type", "application/json");

        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        request.AddStringBody(JsonSerializer.Serialize(payload), ContentType.Json);
        var response = await client.ExecuteAsync(request);

        if (response.IsSuccessful && response.Content != null)
        {
            using var jsonDoc = JsonDocument.Parse(response.Content);
            return jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "No response found.";
        }

        return "Error: Unable to fetch response from Gemini.";
    }
}
