using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace GCTConnect.Services
{
    public class QdrantVectorStore
    {
        private readonly HttpClient _http;
        private readonly string _collection;

        public QdrantVectorStore(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            _collection = cfg["Qdrant:Collection"] ?? "college_kb";

            var endpoint = cfg["Qdrant:Endpoint"]
                           ?? throw new InvalidOperationException("Qdrant endpoint missing");
            _http.BaseAddress = new Uri(endpoint);

            var apiKey = cfg["Qdrant:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                _http.DefaultRequestHeaders.Add("api-key", apiKey);
            }
        }

        // 1️⃣ Create collection if not exists
        public async Task EnsureCollectionAsync(int vectorSize, CancellationToken ct = default)
        {
            var resp = await _http.GetAsync($"/collections/{_collection}", ct);
            if (resp.IsSuccessStatusCode) return; // collection exists

            var body = new
            {
                vectors = new { size = vectorSize, distance = "Cosine" }
            };

            var create = await _http.PutAsJsonAsync($"/collections/{_collection}", body, ct);
            create.EnsureSuccessStatusCode();
        }

        // 2️⃣ Insert or update a chunk
        public async Task UpsertAsync(string id, float[] vector, string text, string source, int index, CancellationToken ct = default)
        {
            var body = new
            {
                points = new[]
                {
                    new {
                        id,
                        vector,
                        payload = new {
                            Text = text,
                            Source = source,
                            Index = index
                        }
                    }
                }
            };

            var resp = await _http.PutAsJsonAsync($"/collections/{_collection}/points", body, ct);
            resp.EnsureSuccessStatusCode();
        }

        // 3️⃣ Search for similar chunks
        public async Task<IReadOnlyList<SearchResult>> SearchAsync(float[] vector, int topK = 5, CancellationToken ct = default)
        {
            var body = new
            {
                vector,
                limit = topK,
                with_payload = true
            };

            var resp = await _http.PostAsJsonAsync($"/collections/{_collection}/points/search", body, ct);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadFromJsonAsync<QdrantSearchResponse>(cancellationToken: ct);
            if (json?.result == null) return Array.Empty<SearchResult>();

            return json.result.Select(r =>
                new SearchResult(
                    r.payload?.Text ?? "",
                    r.payload?.Source ?? "",
                    r.payload?.Index ?? -1,
                    (float)r.score
                )
            ).ToList();
        }
    }

    // 🔹 Models for search results
    public record SearchResult(string Text, string Source, int Index, float Score);

    public class QdrantSearchResponse
    {
        public List<ResultItem>? result { get; set; }

        public class ResultItem
        {
            public double score { get; set; }
            public Payload? payload { get; set; }
        }

        public class Payload
        {
            public string? Text { get; set; }
            public string? Source { get; set; }
            public int Index { get; set; }
        }
    }
}
