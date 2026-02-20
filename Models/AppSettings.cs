using AutoTranslator.Models.Enums;
using System;
using System.IO;
using System.Text.Json.Serialization;

namespace AutoTranslator.Models;

public enum OcrProvider 
{
    HttpApi,
    TesseractLocal
}
public enum LlmProviderType
{
    Ollama,
    Online
}

public class AppSettings
{
    public string ProgramParentFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    [JsonIgnore] public string ProgramFolder => Path.Combine(ProgramParentFolder, "AutoTranslator");
    [JsonIgnore] public string ProjectFolder => Path.Combine(ProgramFolder, "Projects");
    [JsonIgnore] public string AssetFolder => Path.Combine(ProgramFolder, "Assets");
    [JsonIgnore] public string LocalizationFolder => Path.Combine(AssetFolder, "Localization");
    [JsonIgnore] public Localizations Localizations { get; set; }
    public Language InterfaceLanguage { get; set; } = Language.Russian;

    public OcrSettings Ocr { get; set; } = new();
    public LlmSettings Llm { get; set; } = new();
}
public readonly struct Localizations(string localizationPath)
{
    private readonly string _path = localizationPath;
    public string Get(Language language)
    {
        return language switch
        {
            Language.Russian => Russian,
            Language.English => English,
            Language.Japanese => Japanese,
            Language.Korean => Korean,
            Language.Chinese => Chinese,
            Language.German => German,
            Language.French => French,
            Language.Spanish => Spanish,
            _ => throw new NotImplementedException(),
        };
    }
    public readonly string Russian => Path.Combine(_path, "Russian.json");
    public readonly string English => Path.Combine(_path, "English.json");
    public readonly string Japanese => Path.Combine(_path, "Japanese.json");
    public readonly string Korean => Path.Combine(_path, "Korean.json");
    public readonly string Chinese => Path.Combine(_path, "Chinese.json");
    public readonly string German => Path.Combine(_path, "German.json");
    public readonly string French => Path.Combine(_path, "French.json");
    public readonly string Spanish => Path.Combine(_path, "Spanish.json");
}
public class OcrSettings
{
    public OcrProvider Provider { get; set; } = OcrProvider.HttpApi;

    public string? ApiUrl { get; set; }
    public string? ApiKey { get; set; }

    public string? LocalModelPath { get; set; }
}
public class LlmSettings
{
    public LlmProviderType Provider { get; set; }

    // Ollama
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";
    public string OllamaModel { get; set; } = "deepseek-r1:8b";

    // Online
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string OnlineModel { get; set; } = string.Empty;

    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 4096;
}