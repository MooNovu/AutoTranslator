using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class SettingsService : ISettingsService
{
    private const string FileName = "AppSettings.json";
    private readonly string _filePath;
    public AppSettings Settings { get; private set; } = new();

    public SettingsService()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoTranslator");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, FileName);
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            Settings = new AppSettings();
            Settings.Localizations = new(Settings.LocalizationFolder);
            await SaveAsync();
            return;
        }

        var json = await File.ReadAllTextAsync(_filePath);
        Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptionsProvider.Default) ?? new AppSettings();
        Settings.Localizations = new(Settings.LocalizationFolder);
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(Settings, JsonOptionsProvider.Default);

        await File.WriteAllTextAsync(_filePath, json);
    }
}
