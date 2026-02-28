using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;


namespace AutoTranslator.ViewModels.Pages;

public partial class CreateProjectDialogViewModel(IServiceProvider serviceProvider) : ViewModelBase(serviceProvider)
{
    private readonly IProjectService _projectService = serviceProvider.GetRequiredService<IProjectService>();
    private readonly ILocalizationService _localizationService = serviceProvider.GetRequiredService<ILocalizationService>();

    public IEnumerable<Language> AvailableLanguages => Enum.GetValues<Language>();
    /// <summary>
    /// Название проекта
    /// </summary>
    [ObservableProperty]
    private string _projectName = string.Empty;

    /// <summary>
    /// Шрифт проекта
    /// </summary>
    [ObservableProperty]
    private string _fontFileName = string.Empty;

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

    /// <summary>
    /// Событие завершения диалога
    /// </summary>
    public event Action<Project?>? DialogCompleted;

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
        {
            ShowMessage(_localizationService["Error_Set_Project_Name"]);
            return;
        }

        if (SelectedSourceLanguage == SelectedTargetLanguage)
        {
            ShowMessage(_localizationService["Error_Equal_Language"]);
            return;
        }

        IsBusy = true;

        try
        {
            ProjectCreateDTO createDTO;
            if (string.IsNullOrEmpty(FontFileName))
            {
                createDTO = new(ProjectName, ProjectDescription, SelectedSourceLanguage, SelectedTargetLanguage);
            }
            else
            {
                createDTO = new ProjectCreateDTO(ProjectName, ProjectDescription, SelectedSourceLanguage, SelectedTargetLanguage, FontFileName);
            }
             
            var project = await _projectService.CreateProjectAsync(createDTO);

            DialogCompleted?.Invoke(project);
        }
        catch (Exception ex)
        {
            ShowMessage($"{_localizationService["Error_While_Creating_Project"]}: {ex.Message}");
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
