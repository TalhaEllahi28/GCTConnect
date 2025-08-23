using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace GCTConnect.Services
{
    public class GeminiLlmClient
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public GeminiLlmClient(IConfiguration cfg)
        {
            var apiKey = cfg["Llm:ApiKey"]
                         ?? throw new InvalidOperationException("Gemini ApiKey missing");
            _model = cfg["Llm:Model"] ?? "gemini-1.5-flash";

            _http = new HttpClient
            {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
            };
            _http.DefaultRequestHeaders.TryAddWithoutValidation("x-goog-api-key", apiKey);
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
        {
            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var resp = await _http.PostAsJsonAsync($"/v1beta/models/{_model}:generateContent", body, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);
            return json?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text?.Trim()
                   ?? "No response from Gemini";
        }

        private sealed class GeminiResponse
        {
            public List<Candidate>? candidates { get; set; }
            public sealed class Candidate
            {
                public Content? content { get; set; }
            }
            public sealed class Content
            {
                public List<Part>? parts { get; set; }
            }
            public sealed class Part
            {
                public string? text { get; set; }
            }
        }
    }
}
