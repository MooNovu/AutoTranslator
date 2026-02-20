using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class ProjectStatisticsService : IProjectStatisticsService
{
    public Task UpdateAsync(Project project)
    {
        if (!Directory.Exists(project.FolderPath))
            return Task.CompletedTask;

        var chapters = Directory.GetDirectories(project.FolderPath);

        project.ChapterCount = chapters.Length;

        int total = 0;
        int translated = 0;

        foreach (var ch in chapters)
        {
            var originals = Directory.GetFiles(
                ch,
                $"*{ProjectHelper.OriginalImageSuffix}{ProjectHelper.ImageExtension}");

            total += originals.Length;

            translated += originals.Count(o =>
                File.Exists(o.Replace(
                    ProjectHelper.OriginalImageSuffix,
                    ProjectHelper.TranslatedImageSuffix)));
        }

        project.TotalPageCount = total;
        project.TranslationProgress =
            total > 0 ? (double)translated / total * 100 : 0;

        return Task.CompletedTask;
    }
}
