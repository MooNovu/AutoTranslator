using AutoTranslator.Models;
using AutoTranslator.Services.Exception;
using AutoTranslator.Services.Implementations;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.Base;
using AutoTranslator.ViewModels.EditorModels;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTranslator.ViewModels.Pages;
public partial class ProjectEditorViewModel(IServiceProvider sp) : ViewModelBase(sp)
{
    private readonly IChapterService _chapterService = sp.GetRequiredService<IChapterService>();
    private readonly IPageService _pageService = sp.GetRequiredService<IPageService>();
    private readonly ITextEraseService _textEraseService = sp.GetRequiredService<ITextEraseService>();
    private readonly ITranslationOrchestrator _translationOrchestrator = sp.GetRequiredService<ITranslationOrchestrator>();

    private readonly IImageTextRenderer _imageTextRenderer = sp.GetRequiredService<IImageTextRenderer>();
    private readonly IErrorDialogService _errorDialogService = sp.GetRequiredService<IErrorDialogService>();
    private readonly ILocalizationService _localizationService = sp.GetRequiredService<ILocalizationService>();

    [ObservableProperty]
    private bool _isTranslating;

    [ObservableProperty]
    private double _translationProgress;

    [ObservableProperty]
    private string _translationStatus = string.Empty;

    [ObservableProperty]
    private PageStatus _currentViewMode = PageStatus.Original;


    [ObservableProperty]
    private Project? _project;

    public ObservableCollection<ChapterInfo> Chapters { get; } = [];
    public ObservableCollection<PageInfo> Pages { get; } = [];

    [ObservableProperty]
    private ChapterInfo? _selectedChapter;

    [ObservableProperty]
    private PageInfo? _selectedPage;

    [ObservableProperty]
    private Bitmap? _currentImage;

    [RelayCommand]
    private void ShowOriginal()
    {
        CurrentViewMode = PageStatus.Original;
        UpdateCurrentImage();
    }

    [RelayCommand]
    private void ShowCleaned()
    {
        CurrentViewMode = PageStatus.Cleaned;
        UpdateCurrentImage();
    }

    [RelayCommand]
    private void ShowDone()
    {
        CurrentViewMode = PageStatus.Done;
        UpdateCurrentImage();
    }
    private void UpdateCurrentImage()
    {
        CurrentImage?.Dispose();
        if (SelectedPage == null || SelectedChapter == null)
        {
            CurrentImage = null;
            return;
        }

        string? path = CurrentViewMode switch
        {
            PageStatus.Original =>
                _pageService.GetPageImagePath(Project!,
                    SelectedChapter.Number,
                    SelectedPage.Number,
                    PageStatus.Original),

            PageStatus.Cleaned =>
                _pageService.GetPageImagePath(Project!,
                    SelectedChapter.Number,
                    SelectedPage.Number, 
                    PageStatus.Cleaned),

            PageStatus.Done =>
                _pageService.GetPageImagePath(Project!,
                    SelectedChapter.Number,
                    SelectedPage.Number, 
                    PageStatus.Done),

            _ => null
        };

        if (path != null && File.Exists(path))
            CurrentImage = new Bitmap(path);
        else
            CurrentImage = null;
    }

    [RelayCommand]
    private async Task TranslatePageAsync()
    {
        if (!CanTranslate())
            return;

        IsTranslating = true;

        try
        {
            await ExecuteTranslationAsync();
        }
        catch (ServiceException ex)
        {
            await HandleServiceExceptionAsync(ex);
        }
        catch (Exception ex)
        {
            await _errorDialogService.ShowAsync(
                _localizationService["Error_Unknown_Message"],
                ex.Message,
                false);
        }
        finally
        {
            ResetUiState();
        }
    }
    private bool CanTranslate()
    {
        if (Project == null || SelectedChapter == null || SelectedPage == null)
            return false;
        return true;
    }
    private void ResetUiState()
    {
        IsTranslating = false;
        TranslationProgress = 0;
        TranslationStatus = string.Empty;
    }
    private void RenderResult(int chapterNumber, int pageNumber)
    {
        if (SelectedChapter == null || SelectedChapter.Number != chapterNumber) return;

        UpdatePage(chapterNumber, pageNumber);

        if (SelectedPage?.Number != pageNumber) return;

        CurrentViewMode = PageStatus.Done;

        SelectedPage = Pages.FirstOrDefault(p => p.Number == pageNumber);

        UpdateCurrentImage();
    }

    private async Task ExecuteTranslationAsync()
    {
        if (!CanTranslate()) return;

        int TranslatingChapterNumber = SelectedChapter!.Number;
        int TranslatingPageNumber = SelectedPage!.Number;

        var imagePath = _pageService.GetPageImagePath(Project!, TranslatingChapterNumber, TranslatingPageNumber, PageStatus.Original);

        if (imagePath == null) return;

        TranslationStatus = _localizationService["Translation_Status_Recognizing_Text"];
        TranslationProgress = 10;
        var blocks = await _translationOrchestrator.RunOcrAsync(imagePath);

        TranslationStatus = _localizationService["Translation_Status_Erasing_Text"];
        TranslationProgress = 30;
        var cleanedPath = await _textEraseService.EraseTextAsync(imagePath, blocks);

        TranslationStatus = _localizationService["Translation_Status_Translating_Text"];
        TranslationProgress = 70;
        var translatedBlocks = await _translationOrchestrator.
            TranslateBlocksAsync(blocks, Project!.SourceLanguage, Project!.TargetLanguage);

        TranslationStatus = _localizationService["Translation_Status_Saving_Result"];
        TranslationProgress = 95;
        var donePath = Path.Combine(
            Path.GetDirectoryName(imagePath)!,
            ProjectHelper.FromOrigToDone(Path.GetFileName(imagePath)));

        _imageTextRenderer.DrawTextBlocks(cleanedPath, donePath, translatedBlocks);

        await Task.Delay(1000);

        RenderResult(TranslatingChapterNumber, TranslatingPageNumber);

        TranslationStatus = _localizationService["Translation_Status_Completed"];
    }
    private async Task HandleServiceExceptionAsync(ServiceException ex)
    {
        bool retry = await _errorDialogService.ShowAsync(
            "Ошибка",
            ex.Message,
            ex.CanRetry);

        if (retry)
            await TranslatePageAsync();
    }

    public void LoadProject(Project project)
    {
        Project = project;
        LoadChapters();
    }

    private void LoadChapters()
    {
        Chapters.Clear();

        var chapters = _chapterService.GetAllChapters(Project!);

        foreach (var ch in chapters)
        {
            Chapters.Add(new ChapterInfo(
                ch.Number,
                ch.Name,
                ch.PageCount));
        }
    }
    [RelayCommand]
    private async Task CreateChapterAsync()
    {
        if (Project == null)
            return;

        var dialog = new CreateChapterDialog
        {
            DataContext = new CreateChapterDialogViewModel()
        };

        var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!.
            Windows.FirstOrDefault(w => w.IsVisible);
        if (owner == null) return;

        var result = await dialog.ShowDialog<bool?>(owner);

        if (dialog.DataContext is not CreateChapterDialogViewModel vm) return;
        if (!vm.IsConfirmed || string.IsNullOrWhiteSpace(vm.ChapterName)) return;

        var nextNumber = Chapters.Count == 0 ? 1 : Chapters.Max(c => c.Number) + 1;

        await _chapterService.CreateChapterAsync(Project, nextNumber, vm.ChapterName);

        LoadChapters();
    }
    [RelayCommand]
    private async Task DeleteChapterAsync()
    {
        if (Project == null)
            return;

        if (SelectedChapter == null) return;

        var number = SelectedChapter.Number;

        await _chapterService.DeleteChapterAsync(Project, number);

        LoadChapters();
    }

    public async Task AddPageFromFileAsync(string filePath)
    {
        if (Project == null || SelectedChapter == null)
            return;

        await using var fs = File.OpenRead(filePath);

        var nextPage = _pageService.GetNextPageNumber(Project, SelectedChapter.Number);

        await _pageService.AddPageAsync(
            Project,
            SelectedChapter.Number,
            nextPage,
            fs);

        LoadPages(SelectedChapter.Number);
    }

    private void LoadPages(int chapterNumber)
    {
        ClearPages();

        var pages = _pageService.GetPages(Project!, chapterNumber);

        foreach (var p in pages)
        {
            PageInfo info = new(p.Number, p.IsTranslated);

            string? originalPath = _pageService.GetPageImagePath(
                Project!, chapterNumber, p.Number, PageStatus.Original);

            if (originalPath != null && File.Exists(originalPath))
                info.OriginalImage = new Bitmap(originalPath);

            if (p.IsTranslated)
            {
                string? translatedPath = _pageService.GetPageImagePath(
                    Project!, chapterNumber, p.Number, PageStatus.Done);

                if (translatedPath != null && File.Exists(translatedPath))
                    info.TranslatedImage = new Bitmap(translatedPath);
            }

            Pages.Add(info);
        }
    }
    private void UpdatePage(int chapterNumber, int pageNumber)
    {
        PageInfo? pageToUpdate = Pages.FirstOrDefault(p => p.Number == pageNumber);

        if (pageToUpdate == null) return;


        string? translatedPath = _pageService.GetPageImagePath(
            Project!, chapterNumber, pageToUpdate.Number, PageStatus.Done);

        if (translatedPath != null && File.Exists(translatedPath))
        {
            pageToUpdate.TranslatedImage = new Bitmap(translatedPath);
            pageToUpdate.IsTranslated = true;
        }
    }
    private void ClearPages()
    {
        foreach (var page in Pages)
        {
            page.OriginalImage?.Dispose();
            page.TranslatedImage?.Dispose();
        }
        Pages.Clear();
    }
    partial void OnSelectedPageChanged(PageInfo? value)
    {
        UpdateCurrentImage();
    }
    partial void OnSelectedChapterChanged(ChapterInfo? value) 
    { 
        if (value == null) return; 
        LoadPages(value.Number); 
    }
    [RelayCommand]
    private void Refresh()
    {
        if (Project != null)
            LoadChapters();
    }
}