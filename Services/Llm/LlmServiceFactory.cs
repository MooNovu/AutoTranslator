using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace AutoTranslator.Services.Llm;

public class LlmServiceFactory(IServiceProvider sp) : ILlmServiceFactory
{
    private readonly ISettingsService _settings = sp.GetRequiredService<ISettingsService>();
    private readonly IHttpClientFactory _httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    public ILlmService Create()
    {
        var settings = _settings.Settings.Llm;
        var httpClient = _httpClientFactory.CreateClient("LlmClient");

        return settings.Provider switch
        {
            LlmProviderType.Ollama =>
                new OllamaLlmService(
                    httpClient,
                    settings.OllamaEndpoint,
                    settings.OllamaModel,
                    settings.Temperature,
                    settings.MaxTokens),

            LlmProviderType.Online =>
                new OnlineLlmService(
                    httpClient,
                    settings.Endpoint,
                    settings.ApiKey,
                    settings.OnlineModel,
                    settings.Temperature,
                    settings.MaxTokens),

            _ => throw new NotSupportedException()
        };
    }
}
