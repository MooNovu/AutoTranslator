using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.EditorModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public enum PageStatus
{
    Original,
    Cleaned,
    Done
}

public class PageService : IPageService
{
    public async Task AddPageAsync(Project project, int chapter, int page, Stream stream)
    {
        var chapterFolder = FindChapter(project, chapter);

        var fileName = ProjectHelper.FormatPageFile(page, false);
        var path = Path.Combine(chapterFolder, fileName);

        await using var fs = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(fs);
    }
    public int GetNextPageNumber(Project project, int chapterNumber)
    {
        var pages = GetPages(project, chapterNumber);

        if (pages.Count == 0)
            return 1;

        return pages.Max(p => p.Number) + 1;
    }
    public string? GetPageImagePath(
        Project project,
        int chapterNumber,
        int pageNumber,
        PageStatus status)
    {
        var chapterDir = FindChapterDirectory(project, chapterNumber);
        if (chapterDir == null)
            return null;

        var suffix = status switch
        {
            PageStatus.Original => ProjectHelper.OriginalImageSuffix,
            PageStatus.Cleaned => ProjectHelper.CleanedImageSuffix,
            PageStatus.Done => ProjectHelper.TranslatedImageSuffix,
            _ => throw new System.NotImplementedException(),
        };


        var fileName = $"{pageNumber:D3}{suffix}.png";

        var fullPath = Path.Combine(chapterDir, fileName);

        return File.Exists(fullPath)
            ? fullPath
            : null;
    }
    public async Task<bool> MarkTranslatedAsync(Project project, int chapter, int page, Stream stream)
    {
        var chapterFolder = FindChapter(project, chapter);

        var fileName = ProjectHelper.FormatPageFile(page, true);
        var path = Path.Combine(chapterFolder, fileName);

        await using var fs = new FileStream(path, FileMode.Create);
        await stream.CopyToAsync(fs);

        return true;
    }

    private static string FindChapter(Project project, int number)
    {
        return Directory.GetDirectories(project.FolderPath!)
            .First(d => Path.GetFileName(d)
            .StartsWith(number.ToString("D3")));
    }
    private static string? FindChapterDirectory(Project project, int number)
    {
        if (!Directory.Exists(project.FolderPath))
            return null;

        return Directory.GetDirectories(project.FolderPath)
            .FirstOrDefault(d =>
                Path.GetFileName(d)
                    .StartsWith(number.ToString("D3")));
    }

    public List<PageInfo> GetPages(Project project, int chapterNumber)
    {
        var result = new List<PageInfo>();

        var chapterDir = FindChapterDirectory(project, chapterNumber);
        if (chapterDir == null)
            return result;

        var files = Directory.GetFiles(chapterDir, $"*{ProjectHelper.OriginalImageSuffix}.*");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            var numberPart = fileName.Replace(ProjectHelper.OriginalImageSuffix, "");

            if (!int.TryParse(numberPart, out int pageNumber))
                continue;

            var translatedPath = file.Replace(ProjectHelper.OriginalImageSuffix, ProjectHelper.TranslatedImageSuffix);

            result.Add(new PageInfo(pageNumber, File.Exists(translatedPath)));
        }

        return [.. result.OrderBy(p => p.Number)];
    }
}
