using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Ocr;

public class HttpOcrService(HttpClient httpClient, string apiUrl, string? apiKey) : IOcrService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _apiUrl = apiUrl.TrimEnd('/');
    private readonly string? _apiKey = apiKey;

    public async Task<List<OcrWord>> RecognizeAsync(
            string imagePath,
            string lang = "en")
    {
        using var content = new MultipartFormDataContent();

        await using var fs = File.OpenRead(imagePath);
        content.Add(new StreamContent(fs), "file", Path.GetFileName(imagePath));
        content.Add(new StringContent(lang), "lang");

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_apiUrl}/ocr")
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(_apiKey))
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var words = new List<OcrWord>();

        using var doc = JsonDocument.Parse(json);

        foreach (var w in doc.RootElement.GetProperty("words").EnumerateArray())
        {
            words.Add(new OcrWord
            {
                Text = w.GetProperty("text").GetString() ?? "",
                X = w.GetProperty("x").GetInt32(),
                Y = w.GetProperty("y").GetInt32(),
                Width = w.GetProperty("width").GetInt32(),
                Height = w.GetProperty("height").GetInt32(),
                Confidence = w.GetProperty("confidence").GetDouble(),
            });
        }

        return words;
    }
}
