using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AutoTranslator.Views.Pages;
using AutoTranslator.ViewModels.Pages;
using System;
using Microsoft.Extensions.DependencyInjection;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.Services.Implementations;
using AutoTranslator.Services.Llm;
using AutoTranslator.Services.Ocr;
using System.Net.Http;

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

    private static ServiceCollection ConfigureServices()
    {
        ServiceCollection services = new();
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

        services.AddTransient<CreateProjectDialog>();
        services.AddSingleton<HttpClient>();

        services.AddSingleton<IErrorDialogService, ErrorDialogService>();
        services.AddTransient<ErrorDialog>();
        return services;
    }
}