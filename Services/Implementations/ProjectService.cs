using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;

namespace AutoTranslator.Services.Implementations;

public class ProjectService(ISettingsService settings, IProjectStatisticsService statistics) : IProjectService
{
    private readonly ISettingsService _settings = settings;
    private readonly IProjectStatisticsService _statistics = statistics;

    public async Task<Project> CreateProjectAsync(ProjectCreateDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Название проекта не может быть пустым");

        var root = _settings.Settings.ProjectFolder;
        Directory.CreateDirectory(root);

        var safeName = ProjectHelper.SanitizeFileName(dto.Name);
        var folder = ProjectHelper.EnsureUniqueFolder(
            Path.Combine(root, safeName));

        Directory.CreateDirectory(folder);

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            FontFileName = dto.FontFileName,
            Description = dto.Description,
            FolderPath = folder,
            CreatedAt = DateTime.Now,
            LastModified = DateTime.Now,
            SourceLanguage = dto.SourceLanguage,
            TargetLanguage = dto.TargetLanguage
        };

        await SaveProjectAsync(project);
        return project;
    }

    public async Task<Project> LoadProjectAsync(string projectFilePath)
    {
        var json = await File.ReadAllTextAsync(projectFilePath);

        var project = JsonSerializer.Deserialize<Project>(
            json,
            JsonOptionsProvider.Default)
            ?? throw new InvalidOperationException("Ошибка десериализации");

        await _statistics.UpdateAsync(project);
        return project;
    }

    public async Task SaveProjectAsync(Project project)
    {
        project.LastModified = DateTime.Now;

        await _statistics.UpdateAsync(project);

        var filePath = Path.Combine(
            project.FolderPath!,
            $"{project.Id}{ProjectHelper.ProjectFileExtension}");

        var json = JsonSerializer.Serialize(project, JsonOptionsProvider.Default);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<List<Project>> GetAllProjectAsync()
    {
        var root = _settings.Settings.ProjectFolder;

        if (!Directory.Exists(root))
            return [];

        var projects = new List<Project>();

        foreach (var dir in Directory.GetDirectories(root))
        {
            var file = Directory.GetFiles(
                dir,
                $"*{ProjectHelper.ProjectFileExtension}")
                .FirstOrDefault();

            if (file != null)
                projects.Add(await LoadProjectAsync(file));
        }

        return projects
            .OrderByDescending(p => p.LastModified)
            .ToList();
    }

    public Task DeleteProjectAsync(Project project)
    {
        if (Directory.Exists(project.FolderPath))
            Directory.Delete(project.FolderPath, true);

        return Task.CompletedTask;
    }
}