using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace AutoTranslator.Services.Ocr;

public class OcrServiceFactory(IServiceProvider sp, ISettingsService settings) : IOcrServiceFactory
{
    private readonly IServiceProvider _sp = sp;
    private readonly ISettingsService _settings = settings;
    private readonly IHttpClientFactory _httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    public IOcrService Create()
    {
        var settings = _settings.Settings.Ocr;

        if (settings.ApiUrl == null) throw new ArgumentNullException(nameof(settings.ApiUrl));

        return settings.Provider switch
        {
            OcrProvider.HttpApi =>
                new HttpOcrService(_httpClientFactory.CreateClient("OcrClient"), settings.ApiUrl, settings.ApiKey),

            OcrProvider.TesseractLocal => throw new NotImplementedException(),

            _ => throw new NotSupportedException()
        };
    }
}
