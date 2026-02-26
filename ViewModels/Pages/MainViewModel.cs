using AutoTranslator.Models;
using AutoTranslator.Services;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AutoTranslator.ViewModels.Pages;

/// <summary>
/// Главный ViewModel - управляет навигацией между страницами
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    /// <summary>
    /// Текущая отображаемая страница
    /// </summary>
    [ObservableProperty]
    private ViewModelBase? _currentPage;

    public MainViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _windowService = serviceProvider.GetRequiredService<IWindowService>();

        Title = "AutoTranslotor";

        NavigateTo<ProjectSelectionViewModel>();
    }

    [RelayCommand]
    private async Task CreateNewProject()
    {
        var project = await _windowService.OpenNewProjectDialogAsync();

        if (project != null)
        {
            OpenEditor(project);
        }
    }
    [RelayCommand]
    private void ExitApplication()
    {
        //TODO проверка сохранности изменений
        Environment.Exit(0);
    }

    private void OpenEditor(Project? project)
    {
        if (project == null) return;

        _windowService.OpenEditor(project);
        _windowService.CloseCurrentWindow(this);
    }

    /// <summary>
    /// Общий метод навигации
    /// </summary>
    private void NavigateTo<T>() where T : ViewModelBase
    {
        var newPage = ServiceProvider.GetRequiredService<T>();
        if (newPage == null) return;

        if (newPage is SettingsViewModel settingsVm)
        {
            settingsVm.GoBack = () => NavigateTo<ProjectSelectionViewModel>();
        }
        if (newPage is ProjectSelectionViewModel projectVm)
        {
            projectVm.OpenSettings = () => NavigateTo<SettingsViewModel>();
            projectVm.OpenProject = (project) => OpenEditor(project);
            projectVm.OpenCreateProjectDialog = async () => await CreateNewProject();
        }

        CurrentPage?.OnNavigatedFromAsync();

        CurrentPage = newPage;

        newPage.OnNavigatedToAsync();
    }
}
