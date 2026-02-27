using AutoTranslator.Services.Http;
using AutoTranslator.Services.Implementations;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Llm;
using AutoTranslator.Services.Ocr;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.Pages;
using AutoTranslator.Views.Pages;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTranslator;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    public new static App Current => (App)Application.Current!;
    public IServiceProvider? Services { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {

        var services = ConfigureServices();

        _serviceProvider = services.BuildServiceProvider();

        AsyncHelper.RunSync(() => InitializeAsync(_serviceProvider));

        Services = _serviceProvider;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    private async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        await serviceProvider.GetRequiredService<ISettingsService>().LoadAsync();

        var settings = serviceProvider.GetRequiredService<ISettingsService>().Settings;
        serviceProvider.GetRequiredService<ILocalizationService>().SetLanguage(settings.Localizations.Get(settings.InterfaceLanguage));
    }

    private static ServiceCollection ConfigureServices()
    {
        ServiceCollection services = new();

        services.AddHttpClient();

        services.AddHttpClient("OcrClient", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "AutoTranslator/1.0");
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("LlmClient", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "AutoTranslator/1.0");
            client.Timeout = TimeSpan.FromSeconds(60);
        })
            .AddPolicyHandler(HttpClientPolicies.GetRetryPolicy())
            .AddPolicyHandler(HttpClientPolicies.GetTimeoutPolicy());

        // Регистрация ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddTransient<ProjectSelectionViewModel>();
        services.AddTransient<ProjectEditorViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<CreateProjectDialogViewModel>();

        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<IChapterService, ChapterService>();
        services.AddSingleton<IPageService, PageService>();
        services.AddSingleton<IProjectStatisticsService, ProjectStatisticsService>();


        services.AddSingleton<IWindowService, WindowService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IFolderPicker, FolderPickerService>();


        //Заменить на фабрики
        services.AddSingleton<IOcrServiceFactory, OcrServiceFactory>();
        services.AddSingleton<ILlmServiceFactory, LlmServiceFactory>();
        services.AddSingleton<IImageTextRenderer, ImageTextRenderer>();


        services.AddSingleton<ITextEraseService, TextEraseService>();

        services.AddSingleton<ILocalizationService, LocalizationService>();

        services.AddSingleton<ITranslationOrchestrator, TranslationOrchestrator>();

        services.AddTransient<CreateProjectDialog>();

        services.AddSingleton<IErrorDialogService, ErrorDialogService>();
        services.AddTransient<ErrorDialog>();
        return services;
    }
}