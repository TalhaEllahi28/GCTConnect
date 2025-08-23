using System.Text;
using GCTConnect.Services;

namespace GCTConnect.Services
{
    public class RagService
    {
        private readonly QdrantVectorStore _vectorStore;
        private readonly GeminiLlmClient _gemini;
        private readonly OpenAiLlmClient _openai;
        private readonly IConfiguration _cfg;

        public RagService(QdrantVectorStore vectorStore, GeminiLlmClient gemini, OpenAiLlmClient openai, IConfiguration cfg)
        {
            _vectorStore = vectorStore;
            _gemini = gemini;
            _openai = openai;
            _cfg = cfg;
        }

        public async Task<string> AskAsync(string question)
        {
            // step 1: embed question (placeholder for now)
            var fakeEmbedding = new float[1536]; // TODO: replace with real embeddings

            // step 2: search in Qdrant
            var results = await _vectorStore.SearchAsync(fakeEmbedding, 3);
            var context = string.Join("\n\n", results.Select(r => r.Text));

            // step 3: build prompt
            var prompt = $@"
You are a helpful chat bot assistant for college queries.
Use the provided context to answer.

Context:
{context}

Question: {question}
Answer:";

            // step 4: call chosen LLM
            var provider = _cfg["Llm:Provider"];
            if (provider == "Gemini")
                return await _gemini.GenerateAsync(prompt);
            else
                return await _openai.GenerateAsync(prompt);
        }
    }
}
