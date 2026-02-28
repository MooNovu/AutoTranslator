using AutoTranslator.Models.Enums;
using System;

namespace AutoTranslator.Models;

/// <summary>
/// Модель проекта перевода комикса
/// </summary>
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? FontFileName { get; set; }
    public string? Description { get; set; }
    public string? FolderPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public int ChapterCount { get; set; }
    public int TotalPageCount { get; set; }
    public double TranslationProgress { get; set; }
    public Language SourceLanguage { get; set; } = Language.Japanese;
    public Language TargetLanguage { get; set; } = Language.Russian;
    public string LastModifiedFormatted => LastModified.ToString("dd.MM.yyyy HH:mm");
}
