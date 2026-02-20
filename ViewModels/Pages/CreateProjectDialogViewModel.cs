using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace AutoTranslator.ViewModels.Pages;

public partial class CreateProjectDialogViewModel : ViewModelBase
{
    private readonly IProjectService _projectService;

    public IEnumerable<Language> AvailableLanguages => Enum.GetValues<Language>();
    /// <summary>
    /// Название проекта
    /// </summary>
    [ObservableProperty]
    private string _projectName = string.Empty;

    /// <summary>
    /// Описание проекта
    /// </summary>
    [ObservableProperty]
    private string _projectDescription = string.Empty;

    /// <summary>
    /// Выбранный исходный язык
    /// </summary>
    [ObservableProperty]
    private Language _selectedSourceLanguage = Language.Japanese;

    /// <summary>
    /// Выбранный целевой язык
    /// </summary>
    [ObservableProperty]
    private Language _selectedTargetLanguage = Language.Russian;

    public CreateProjectDialogViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        _projectService = serviceProvider.GetRequiredService<IProjectService>();
        Title = "Создание нового проекта";
    }

    /// <summary>
    /// Событие завершения диалога
    /// </summary>
    public event Action<Project?>? DialogCompleted;

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ShowMessage("Введите название проекта");
            return;
        }

        if (SelectedSourceLanguage == SelectedTargetLanguage)
        {
            ShowMessage("Исходный и целевой язык не должны совпадать");
            return;
        }

        IsBusy = true;

        try
        {
            var createDTO = new ProjectCreateDTO(ProjectName, ProjectDescription, SelectedSourceLanguage, SelectedTargetLanguage);
            var project = await _projectService.CreateProjectAsync(createDTO);

            Debug.WriteLine($"Проект '{ProjectName}' успешно создан!");
            ShowMessage($"Проект '{ProjectName}' успешно создан!");

            DialogCompleted?.Invoke(project);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка при создании проекта: {ex.Message}");
            ShowMessage($"Ошибка при создании проекта: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogCompleted?.Invoke(null);
    }
}
