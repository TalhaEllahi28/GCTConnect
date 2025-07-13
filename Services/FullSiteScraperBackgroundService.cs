//using GCTConnect.Services;
//using System.Text.Json;
//using System;
//using System.IO;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Hosting;
//using System.Text.RegularExpressions;
//using GCTConnect.Interfaces;
//using Microsoft.Extensions.DependencyInjection;

//namespace GCTConnect.Services
//{
//    public class FullSiteScraperBackgroundService : BackgroundService
//    {
//private readonly TextProcessingService _textProcessingService;
//        public FullSiteScraperBackgroundService(TextProcessingService textProcessingService)
//        {
//            _textProcessingService = textProcessingService;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            string baseUrl = "http://gctownship.edu.pk/"; 
//            FullSiteScraper scraper = new FullSiteScraper(baseUrl, maxDepth: 3);

//            //var scrapedData = await scraper.ScrapeAllPagesAsync(baseUrl);

//            // Save the data to a JSON file
//            string filePath = "scraped_data.json";
//            //await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(scrapedData, new JsonSerializerOptions { WriteIndented = true }));


//            //_textProcessingService.ProcessScrapedFileAsync(filePath);

//            //string jsonData = await File.ReadAllTextAsync(filePath);
//            //var scrapedData = JsonSerializer.Deserialize<List<string>>(jsonData);

//            //using (var scope = _scopeFactory.CreateScope())
//            //{
//            //    //var textProcessingService = scope.ServiceProvider.GetRequiredService<ITextProcessingService>();

//            //    // Call the ProcessScrapedData method safely within the scope
//            //    var processedData = textProcessingService.ProcessScrapedData(scrapedData);
//            //}

//            Console.WriteLine($"Scraping completed! Data saved in {filePath}");
//        }

//        public static string CleanText(string text)
//        {
//            text = Regex.Replace(text, @"\s+", " "); // Remove extra spaces
//            text = Regex.Replace(text, @"<.*?>", ""); // Remove any leftover HTML
//            text = text.Trim();
//            return text;
//        }
//    }
//}
