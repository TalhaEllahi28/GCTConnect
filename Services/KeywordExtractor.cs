using System;
using System.Collections.Generic;
using System.Linq;

namespace GCTConnect.Services
{
    public class KeywordExtractor
    {
        private readonly StopWordsHandler _stopWordsHandler = new StopWordsHandler();

        /// <summary>
        /// Extracts the most important word from the text using a frequency-based approach.
        /// </summary>
        public string ExtractMostImportantWord(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "Uncategorized";

            // Remove stopwords and lower the text.
            string cleanedText = _stopWordsHandler.RemoveStopWords(text.ToLowerInvariant());

            // Split the cleaned text into words.
            var words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return "Uncategorized";

            // Count frequency of each word.
            var frequency = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (frequency.ContainsKey(word))
                    frequency[word]++;
                else
                    frequency[word] = 1;
            }

            // Return the word with the highest frequency.
            var mostImportant = frequency.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
            return mostImportant ?? "Uncategorized";
        }
    }
}
