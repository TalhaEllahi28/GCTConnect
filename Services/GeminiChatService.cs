using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
using System.Text.Json;

// Services/IGeminiService.cs
public interface IGeminiService
{
    Task<string> GetResponseAsync(string userMessage);
}


// Services/GeminiService.cs
public class GeminiService : IGeminiService
{
    //Attributes of geminiService class
    private readonly string _apiKey;  
    private readonly string _model;
    private readonly string _collegeData;


    //constructor
    public GeminiService(IConfiguration configuration, IWebHostEnvironment env)
    {
        //values taken from appsetting.josn file
        _apiKey = configuration["Llm:ApiKey"];
        _model = configuration["Llm:Model"] ?? "gemini-1.5-flash";

        // Load college data from JSON file
        var collegeDataPath = Path.Combine(env.ContentRootPath, "CollegeData.json");
        _collegeData = File.ReadAllText(collegeDataPath);
    }

    public async Task<string> GetResponseAsync(string userMessage)
    {
        using var httpClient = new HttpClient();

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = $"You are an AI assistant for a college. Use the following college data to answer questions: {_collegeData}. " +
                                   $"User question: {userMessage}. " +
                                   $"Provide helpful, accurate information about college-related queries including admissions, courses, faculty, campus facilities, events, etc. " +
                                   $"Keep responses concise and focused on college information. If asked about something unrelated to college, politely redirect to college topics."
                        }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 1024,
            }
        };

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";

        var response = await httpClient.PostAsJsonAsync(url, requestBody);
        response.EnsureSuccessStatusCode();

        var responseData = await response.Content.ReadFromJsonAsync<GeminiResponse>();

        return responseData?.Candidates?[0]?.Content?.Parts?[0]?.Text
            ?? "I'm sorry, I couldn't process your request at the moment.";
    }
}








// Models for Gemini response
public class GeminiResponse
{
    public Candidate[] Candidates { get; set; }
}

public class Candidate
{
    public Content Content { get; set; }
}

public class Content
{
    public Part[] Parts { get; set; }
}

public class Part
{
    public string Text { get; set; }
}
