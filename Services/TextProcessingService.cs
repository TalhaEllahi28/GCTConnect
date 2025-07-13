//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text.Json;
//using System.Threading.Tasks;
//using GCTConnect.Interfaces;
//using GCTConnect.Models;
//using Microsoft.EntityFrameworkCore;

//namespace GCTConnect.Services
//{
//    public class TextProcessingService
//    {
//        private readonly OpenAIService _openAIService;
//        private readonly GctConnectContext _dbContext;

//        public TextProcessingService(OpenAIService openAIService, GctConnectContext dbContext)
//        {
//            _openAIService = openAIService;
//            _dbContext = dbContext;
//        }

//        public async Task ProcessScrapedFileAsync()
//        {
//            string filePath= "scraped_data.json";
//            if (!System.IO.File.Exists(filePath))
//                throw new FileNotFoundException("Scraped file not found.");

//            // Read scraped JSON file (List<string>)
//            var jsonData = await System.IO.File.ReadAllTextAsync(filePath);
//            var scrapedData = JsonSerializer.Deserialize<List<string>>(jsonData);

//            if (scrapedData == null || !scrapedData.Any())
//                throw new Exception("No valid data found in the scraped file.");

//            var processedData = new List<CollegeDatum>();

//            foreach (var text in scrapedData)
//            {
//                if (string.IsNullOrWhiteSpace(text)) continue;

//                try
//                {
//                    // Extract keyword category using OpenAI
//                    string category = await _openAIService.ExtractKeywordsAsync(text);

//                    // Store in database
//                    var collegeDatum = new CollegeDatum { Category = category, Data = text };
//                    processedData.Add(collegeDatum);
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine($"Error processing text: {text}. Exception: {ex.Message}");
//                }
//            }

//            if (processedData.Any())
//            {
//                await _dbContext.CollegeData.AddRangeAsync(processedData);
//                await _dbContext.SaveChangesAsync();
//            }
//        }
//    }
//}
