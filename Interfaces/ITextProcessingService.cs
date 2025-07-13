using GCTConnect.Models;
using System.Collections.Generic;

namespace GCTConnect.Interfaces
{
    public interface ITextProcessingService
    {
        (string CleanedText, string MostImportantWord) ProcessText(string inputText);
        List<CollegeDatum> ProcessScrapedFileAsync(List<string> rawData);
    }
}
