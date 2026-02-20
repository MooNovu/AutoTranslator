using AutoTranslator.Models.DTO;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Llm;

public class OnlineLlmService(
    HttpClient httpClient,
    string endpoint,
    string apiKey,
    string model,
    double temperature,
    int maxTokens) : ILlmService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _endpoint = endpoint;
    private readonly string _apiKey = apiKey;
    private readonly string _model = model;
    private readonly double _temperature = temperature;
    private readonly int _maxTokens = maxTokens;

    public async Task<LlmResponse> TranslateAsync(LlmRequest request)
    {
        var body = new
        {
            model = _model,
            temperature = _temperature,
            max_tokens = _maxTokens,
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = JsonContent.Create(body)
        };

        if (!string.IsNullOrWhiteSpace(_apiKey))
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var content = json
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return LlmResponseParser.Parse(content ?? string.Empty);
    }
}