using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace GCTConnect.Services
{

    public class FullSiteScraper
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly HashSet<string> _visitedUrls = new HashSet<string>(); // Prevents duplicate scraping
        private readonly string _baseDomain;
        private readonly int _maxDepth = 3; // Limits recursion to prevent excessive requests

        public FullSiteScraper(string baseUrl, int maxDepth = 3)
        {
            _baseDomain = new Uri(baseUrl).Host;
            _maxDepth = maxDepth;
        }

        public async Task<List<string>> ScrapeAllPagesAsync(string startUrl)
        {
            var allPagesContent = new List<string>();
            await ScrapePageAsync(startUrl, 0, allPagesContent);
            return allPagesContent;
          }

        private async Task ScrapePageAsync(string url, int depth, List<string> allPagesContent)
        {
            if (depth > _maxDepth || _visitedUrls.Contains(url))
                return;

            _visitedUrls.Add(url);

            try
            {
                var html = await _httpClient.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                // Extract and store text content
                var contentNodes = doc.DocumentNode.SelectNodes("//p | //h1 | //h2 | //h3 | //h4 | //div[@class='content']");
                if (contentNodes != null)
                {
                    foreach (var node in contentNodes)
                    {
                        var text = node.InnerText.Trim();
                        if (!string.IsNullOrEmpty(text))
                        {
                            allPagesContent.Add(text);
                        }
                    }
                }

                // Extract all links from the page
                var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linkNodes != null)
                {
                    foreach (var link in linkNodes)
                    {
                        var href = link.GetAttributeValue("href", string.Empty);
                        if (!string.IsNullOrEmpty(href))
                        {
                            var absoluteUrl = NormalizeUrl(url, href);
                            if (absoluteUrl != null && new Uri(absoluteUrl).Host == _baseDomain)
                            {
                                await ScrapePageAsync(absoluteUrl, depth + 1, allPagesContent);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping {url}: {ex.Message}");
            }
        }

        private string? NormalizeUrl(string baseUrl, string relativeUrl)
        {
            try
            {
                if (relativeUrl.StartsWith("http"))
                    return relativeUrl;
                return new Uri(new Uri(baseUrl), relativeUrl).ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}


