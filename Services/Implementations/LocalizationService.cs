using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class LocalizationService : ILocalizationService
{
    private Dictionary<string, string> _strings = [];

    public event PropertyChangedEventHandler? PropertyChanged;

    public string this[string key]
    {
        get
        {
            if (_strings.TryGetValue(key, out var value))
                return value;

            return $"??{key}??";
        }
    }

    public void SetLanguage(string languageFilePath)
    {
        if (!File.Exists(languageFilePath))
            return;

        var json = File.ReadAllText(languageFilePath);
        _strings = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptionsProvider.Default) ?? [];

        Invalidate();
    }

    private void Invalidate()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item"));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
    }
}
