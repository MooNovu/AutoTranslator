using AutoTranslator.Models;
using AutoTranslator.Services;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.ViewModels.Base;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AutoTranslator.ViewModels.Pages;

public partial class ProjectSelectionViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;

    public Action? OpenSettings { get; set; }
    public Action? OpenCreateProjectDialog { get; set; }
    public Action<Project?>? OpenProject { get; set; }


    [ObservableProperty]
    private ObservableCollection<Project> _projects = [];

    [ObservableProperty]
    private Project? _selectedProject;


    [RelayCommand]
    private void CreateProject()
    {
        OpenCreateProjectDialog?.Invoke();
    }

    [RelayCommand]
    private void GoToSettings()
    {
        OpenSettings?.Invoke();
    }

    public ProjectSelectionViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _projectService = serviceProvider.GetRequiredService<IProjectService>();
        Title = "Выбор проекта";
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadProjectsAsync();
    }
    private async Task LoadProjectsAsync()
    {
        IsBusy = true;
        StatusMessage = "Загрузка проектов...";

        try
        {
            var projects = await _projectService.GetAllProjectAsync();

            Projects.Clear();
            foreach (var project in projects)
            {
                Projects.Add(project);
            }

            StatusMessage = $"Загружено {Projects.Count} проектов";
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка загрузки проектов: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Открыть выбранный проект
    /// </summary>
    [RelayCommand]
    private void OpenSelectedProject()
    {
        if (SelectedProject == null)
        {
            ShowMessage("Выберите проект для открытия");
            return;
        }

        OpenProject?.Invoke(SelectedProject);
    }

    /// <summary>
    /// Обновить список проектов
    /// </summary>
    [RelayCommand]
    private async Task RefreshProjectsAsync()
    {
        await LoadProjectsAsync();
    }

    /// <summary>
    /// Удалить выбранный проект
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedProjectAsync()
    {
        if (SelectedProject == null)
        {
            ShowMessage("Выберите проект для удаления");
            return;
        }

        // TODO: Добавить диалог подтверждения

        try
        {
            await _projectService.DeleteProjectAsync(SelectedProject);
            Projects.Remove(SelectedProject);
            SelectedProject = null;

            ShowMessage("Проект успешно удален");
        }
        catch (Exception ex)
        {
            ShowMessage($"Ошибка при удалении проекта: {ex.Message}");
        }
    }
}
