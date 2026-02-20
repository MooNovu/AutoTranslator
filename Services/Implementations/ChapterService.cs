using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoTranslator.Services.Static;
using AutoTranslator.ViewModels.EditorModels;
using System.Diagnostics;

namespace AutoTranslator.Services.Implementations;

public class ChapterService : IChapterService
{
    public Task CreateChapterAsync(Project project, int number, string name)
    {
        var folderName = ProjectHelper.FormatChapterFolder(number, name);
        var path = Path.Combine(project.FolderPath!, folderName);

        Directory.CreateDirectory(path);
        return Task.CompletedTask;
    }

    public async Task DeleteChapterAsync(Project project, int number)
    {
        ArgumentNullException.ThrowIfNull(project);

        if (!Directory.Exists(project.FolderPath))
            return;

        var chapterFolder = Directory.GetDirectories(project.FolderPath)
            .FirstOrDefault(d => Path.GetFileName(d)
                .StartsWith(number.ToString("D3")));

        if (chapterFolder != null && Directory.Exists(chapterFolder))
            Directory.Delete(chapterFolder, true);

        await Task.CompletedTask;
    }

    public List<ChapterInfo> GetAllChapters(Project project)
    {
        var result = new List<ChapterInfo>();

        if (!Directory.Exists(project.FolderPath))
            return result;

        var chapterDirs = Directory.GetDirectories(project.FolderPath);

        foreach (var dir in chapterDirs)
        {
            var folderName = Path.GetFileName(dir);

            // формат: 001 - Название
            var parts = folderName.Split("_", StringSplitOptions.None);
            if (parts.Length < 2)
                continue;

            if (!int.TryParse(parts[0], out int number))
                continue;

            var name = parts[1];

            var pageCount = Directory
                .GetFiles(dir, "*_orig.*")
                .Length;

            
            result.Add(new ChapterInfo(number, name, pageCount));
        }

        return [.. result.OrderBy(c => c.Number)];
    }
}
