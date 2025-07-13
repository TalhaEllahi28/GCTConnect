//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using GCTConnect.Interfaces;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;

//namespace GCTConnect.Services
//{
//    public class ScrapedDataWorker : BackgroundService
//    {
//        private readonly IServiceProvider _serviceProvider;
//        private readonly ILogger<ScrapedDataWorker> _logger;

//        public ScrapedDataWorker(IServiceProvider serviceProvider, ILogger<ScrapedDataWorker> logger)
//        {
//            _serviceProvider = serviceProvider;
//            _logger = logger;
//        }

//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            _logger.LogInformation("Scraped Data Processing Worker started.");

//            while (!stoppingToken.IsCancellationRequested)
//            {
//                try
//                {
//                    using (var scope = _serviceProvider.CreateScope())
//                    {
//                        var textProcessingService = scope.ServiceProvider.GetRequiredService<TextProcessingService>();
//                        await textProcessingService.ProcessScrapedFileAsync();
//                    }

//                    _logger.LogInformation("Scraped data processed successfully.");

//                    // Set delay before the next execution (e.g., every 1 hour)
//                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError($"Error in background worker: {ex.Message}");
//                }
//            }
//        }
//    }
//}
