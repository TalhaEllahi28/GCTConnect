using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace GCTConnect.Services
{
    public class OpenAiLlmClient
    {
        private readonly HttpClient _http;
        private readonly string _model;

        public OpenAiLlmClient(IConfiguration cfg)
        {
            var apiKey = cfg["Llm:ApiKey"]
                         ?? throw new InvalidOperationException("OpenAI ApiKey missing");
            _model = cfg["Llm:Model"] ?? "gpt-4o-mini"; // or gpt-4o, gpt-3.5-turbo, etc.

            _http = new HttpClient
            {
                BaseAddress = new Uri("https://api.openai.com/v1/")
            };
            _http.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken ct = default)
        {
            var body = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful college assistant." },
                    new { role = "user", content = prompt }
                }
            };

            var resp = await _http.PostAsJsonAsync("chat/completions", body, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken: ct);
            return json?.choices?.FirstOrDefault()?.message?.content?.Trim()
                   ?? "No response from ChatGPT";
        }

        private sealed class OpenAiResponse
        {
            public List<Choice>? choices { get; set; }
            public sealed class Choice
            {
                public Message? message { get; set; }
            }
            public sealed class Message
            {
                public string? role { get; set; }
                public string? content { get; set; }
            }
        }
    }
}
