using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace AutoTranslator.ViewModels.Pages;
public partial class ProjectEditorViewModel(IServiceProvider sp) : ViewModelBase(sp)
{
    private readonly IChapterService _chapterService = sp.GetRequiredService<IChapterService>();
    private readonly IPageService _pageService = sp.GetRequiredService<IPageService>();
    private readonly ITextEraseService _textEraseService = sp.GetRequiredService<ITextEraseService>();

    private readonly IOcrServiceFactory _ocrFactory = sp.GetRequiredService<IOcrServiceFactory>();
    private readonly ILlmServiceFactory _llmFactory = sp.GetRequiredService<ILlmServiceFactory>();
    private readonly IImageTextRenderer _imageTextRenderer = sp.GetRequiredService<IImageTextRenderer>();
    private readonly IErrorDialogService _errorDialogService = sp.GetRequiredService<IErrorDialogService>();

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
                "Неизвестная ошибка",
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
    private void RenderResult(string cleanedPath, string originalPath, List<MergedBlock> translatedBlocks)
    {
        if (SelectedChapter == null || SelectedPage == null)
            return;

        var donePath = Path.Combine(
            Path.GetDirectoryName(originalPath)!,
            ProjectHelper.FromOrigToDone(Path.GetFileName(originalPath)));

        _imageTextRenderer.DrawTextBlocks(
            cleanedPath,
            donePath,
            translatedBlocks);

        // Помечаем страницу как переведённую
        //_pageService.MarkTranslatedAsync(
        //    Project!,
        //    SelectedChapter.Number,
        //    SelectedPage.Number,
        //    donePath).Wait();

        SelectedPage.IsTranslated = true;

        int? selectedPageNumber = SelectedPage?.Number;

        LoadPages(SelectedChapter.Number);
        // Переключаемся на Done
        if (selectedPageNumber.HasValue)
        {
            SelectedPage = Pages.FirstOrDefault(p => p.Number == selectedPageNumber.Value);
        }

        CurrentViewMode = PageStatus.Done;
        UpdateCurrentImage();
    }

    private async Task ExecuteTranslationAsync()
    {
        IOcrService ocrService = _ocrFactory.Create();
        ILlmService llmService = _llmFactory.Create();

        var imagePath = _pageService.GetPageImagePath(Project!, SelectedChapter!.Number, SelectedPage!.Number, PageStatus.Original);

        if (imagePath == null) return;
        TranslationStatus = "OCRing";
        TranslationProgress = 10;
        var blocks = await RunOcrAsync(ocrService, imagePath);

        TranslationStatus = "Cleaning";
        TranslationProgress = 30;
        var cleanedPath = await _textEraseService.EraseTextAsync(imagePath, blocks);

        TranslationStatus = "LLM translating";
        TranslationProgress = 70;
        var translatedBlocks = await TranslateBlocksWithRetryAsync(
            llmService,
            blocks);

        RenderResult(cleanedPath, imagePath, translatedBlocks);

        TranslationStatus = "Done";
    }
    private async Task<List<MergedBlock>> TranslateBlocksWithRetryAsync(ILlmService llmService, List<MergedBlock> blocks)
    {
        while (true)
        {
            try
            {
                var request = new LlmRequest
                {
                    SystemPrompt = ProjectHelper.SystemPrompt(Project!.SourceLanguage, Project!.TargetLanguage),
                    UserPrompt = ProjectHelper.UserPrompt(blocks)
                };

                LlmResponse response = await llmService.TranslateAsync(request);

                if (response.TranslatedBlocks.Count < blocks.Count)
                    throw new LlmException("LLM вернул неполный перевод.", true);

                return MapTranslatedBlocks(blocks, response.TranslatedBlocks);
            }
            catch (LlmException ex)
            {
                bool retry = await _errorDialogService.ShowAsync(
                    "Ошибка перевода",
                    ex.Message,
                    ex.CanRetry);

                if (!retry)
                    throw;
            }
        }
    }
    private async Task<List<MergedBlock>> RunOcrAsync(IOcrService ocrService, string imagePath)
    {
        var words = await ocrService.RecognizeAsync(imagePath);

        var realWords = words
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .ToList();

        return OcrGrouping.MergeTextBlocks(realWords);
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
    private List<MergedBlock> MapTranslatedBlocks(List<MergedBlock> blocks, List<string> translations)
    {
        List<MergedBlock> result = [];
        for (int i = 0; i < blocks.Count; i++) 
        { 
            MergedBlock b = new() 
            { 
                Text = translations[i], 
                Bounds = blocks[i].Bounds, 
                Confidence = blocks[i].Confidence, 
                OriginalBlocks = blocks[i].OriginalBlocks, 
            };
            result.Add(b); 
        }
        return result;
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
        Pages.Clear();

        var pages = _pageService.GetPages(Project!, chapterNumber);

        foreach (var p in pages)
        {
            var info = new PageInfo(p.Number, p.IsTranslated);

            var originalPath = _pageService.GetPageImagePath(
                Project!, chapterNumber, p.Number, PageStatus.Original);

            if (originalPath != null && File.Exists(originalPath))
                info.OriginalImage = new Bitmap(originalPath);

            if (p.IsTranslated)
            {
                var translatedPath = _pageService.GetPageImagePath(
                    Project!, chapterNumber, p.Number, PageStatus.Done);

                if (translatedPath != null && File.Exists(translatedPath))
                    info.TranslatedImage = new Bitmap(translatedPath);
            }

            Pages.Add(info);
        }
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

    //[RelayCommand]
    //private async Task MarkAsTranslatedAsync()
    //{
    //    if (SelectedChapter == null || SelectedPage == null)
    //        return;

    //    await _pageService.MarkTranslatedAsync(
    //        Project!,
    //        SelectedChapter.Number,
    //        SelectedPage.Number, null);

    //    SelectedPage.IsTranslated = true;
    //}

    [RelayCommand]
    private void Refresh()
    {
        if (Project != null)
            LoadChapters();
    }
}