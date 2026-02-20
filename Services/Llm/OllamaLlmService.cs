using AutoTranslator.Models.DTO;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Llm;

public class OllamaLlmService(
    HttpClient httpClient,
    string endpoint,
    string model,
    double temperature,
    int maxTokens) : ILlmService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly string _endpoint = endpoint.TrimEnd('/');
    private readonly string _model = model;
    private readonly double _temperature = temperature;
    private readonly int _maxTokens = maxTokens;

    public async Task<LlmResponse> TranslateAsync(LlmRequest request)
    {
        var body = new
        {
            model = _model,
            stream = false,
            options = new
            {
                temperature = _temperature,
                num_predict = _maxTokens,
            },
            messages = new[]
            {
                new { role = "system", content = request.SystemPrompt },
                new { role = "user", content = request.UserPrompt }
            }
        };

        var url = $"{_endpoint}/api/chat";

        var response = await _httpClient.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        var content = json
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return LlmResponseParser.Parse(content ?? string.Empty);
    }
}
