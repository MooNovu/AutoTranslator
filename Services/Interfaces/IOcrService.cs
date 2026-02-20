using AutoTranslator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IOcrService
{
    Task<List<OcrWord>> RecognizeAsync(string imagePath, string lang = "en");
}
