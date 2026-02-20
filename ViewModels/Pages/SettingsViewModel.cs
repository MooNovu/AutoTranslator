using AutoTranslator.Models;
using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    public Action? GoBack { get; set; }

    [ObservableProperty]
    private string _defaultProjectsFolder;

    [ObservableProperty]
    private Language _interfaceLanguage;

    [ObservableProperty]
    private OcrProvider _ocrProvider;

    [ObservableProperty]
    private string? _ocrApiUrl;

    [ObservableProperty]
    private string? _ocrApiKey;

    [ObservableProperty]
    private string? _localOcrModelPath;

    [ObservableProperty]
    private LlmProviderType _llmProvider;

    [ObservableProperty]
    private string _ollamaModel;

    [ObservableProperty]
    private string _ollamaEndpoint;

    [ObservableProperty]
    private string _apiKey;

    [ObservableProperty]
    private string _endpoint;

    [ObservableProperty]
    private string _onlineModel;

    [ObservableProperty]
    private double _temperature;

    [ObservableProperty]
    private int _maxTokens;

    public IEnumerable<Language> AvailableLanguages { get; } = Enum.GetValues<Language>();
    public IEnumerable<OcrProvider> AvailableOcrProviders { get; } = Enum.GetValues<OcrProvider>();
    public IEnumerable<LlmProviderType> AvailableLlmProviders { get; } = Enum.GetValues<LlmProviderType>();
    public IEnumerable<string> OllamaModels { get; } = ProjectHelper.GetOllamaModels();
    public bool IsOllamaSelected => LlmProvider == LlmProviderType.Ollama;
    public bool IsOnlineSelected => LlmProvider == LlmProviderType.Online;

    partial void OnLlmProviderChanged(LlmProviderType value)
    {
        OnPropertyChanged(nameof(IsOllamaSelected));
        OnPropertyChanged(nameof(IsOnlineSelected));
    }

    public bool IsHttpOcrSelected => OcrProvider == OcrProvider.HttpApi;
    public bool IsLocalOcrSelected => OcrProvider == OcrProvider.TesseractLocal;

    partial void OnOcrProviderChanged(OcrProvider value)
    {
        OnPropertyChanged(nameof(IsHttpOcrSelected));
        OnPropertyChanged(nameof(IsLocalOcrSelected));
    }

    private readonly ISettingsService _settingsService;
    private readonly ILocalizationService _localizationService;
    private readonly IFolderPicker _folderPicker;
    public SettingsViewModel(IServiceProvider sp) : base(sp)
    {
        _settingsService = sp.GetRequiredService<ISettingsService>();
        _localizationService = sp.GetRequiredService<ILocalizationService>();
        _folderPicker = sp.GetRequiredService<IFolderPicker>();

        Title = "Настройки";

        DefaultProjectsFolder = _settingsService.Settings.ProgramParentFolder;
        InterfaceLanguage = _settingsService.Settings.InterfaceLanguage;
        OcrProvider = _settingsService.Settings.Ocr.Provider;
        LlmProvider = _settingsService.Settings.Llm.Provider;
        OllamaModel = _settingsService.Settings.Llm.OllamaModel;
        OllamaEndpoint = _settingsService.Settings.Llm.OllamaEndpoint;
        ApiKey = _settingsService.Settings.Llm.ApiKey;
        Endpoint = _settingsService.Settings.Llm.Endpoint;
        OnlineModel = _settingsService.Settings.Llm.OnlineModel;
        Temperature = _settingsService.Settings.Llm.Temperature;
        MaxTokens = _settingsService.Settings.Llm.MaxTokens;
        OcrProvider = _settingsService.Settings.Ocr.Provider;
        OcrApiUrl = _settingsService.Settings.Ocr.ApiUrl;
        OcrApiKey = _settingsService.Settings.Ocr.ApiKey;
        LocalOcrModelPath = _settingsService.Settings.Ocr.LocalModelPath;
    }

    [RelayCommand]
    private void Back()
    {
        var lang = _settingsService.Settings.InterfaceLanguage;
        _localizationService.SetLanguage(_settingsService.Settings.Localizations.Get(lang));
        GoBack?.Invoke();
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        IsBusy = true;
        var settings = _settingsService.Settings;

        settings.ProgramParentFolder = DefaultProjectsFolder;
        settings.InterfaceLanguage = InterfaceLanguage;

        _localizationService.SetLanguage(settings.Localizations.Get(InterfaceLanguage));

        settings.ProgramParentFolder = DefaultProjectsFolder;
        settings.InterfaceLanguage = InterfaceLanguage;

        settings.Ocr.Provider = OcrProvider;

        settings.Llm.Provider = LlmProvider;
        settings.Llm.OllamaModel = OllamaModel;
        settings.Llm.OllamaEndpoint = OllamaEndpoint;

        settings.Llm.ApiKey = ApiKey;
        settings.Llm.Endpoint = Endpoint;
        settings.Llm.OnlineModel = OnlineModel;

        settings.Llm.Temperature = Temperature;
        settings.Llm.MaxTokens = MaxTokens;

        settings.Ocr.Provider = OcrProvider;
        settings.Ocr.ApiUrl = OcrApiUrl;
        settings.Ocr.ApiKey = OcrApiKey;
        settings.Ocr.LocalModelPath = LocalOcrModelPath;

        await _settingsService.SaveAsync();

        IsBusy = false;
        ShowMessage("Настройки сохранены");
        GoBack?.Invoke();
    }

    partial void OnInterfaceLanguageChanged(Language value)
    {
        _localizationService.SetLanguage(_settingsService.Settings.Localizations.Get(value));
    }

    [RelayCommand]
    private async Task SelectProjectsFolderAsync()
    {
        var folder = await _folderPicker.PickFolderAsync();

        if (!string.IsNullOrEmpty(folder)) DefaultProjectsFolder = folder;
    }

    [RelayCommand]
    private async Task SelectOcrModelPathAsync()
    {
        var folder = await _folderPicker.PickFolderAsync();
        if (!string.IsNullOrEmpty(folder))
            LocalOcrModelPath = folder;
    }
}
