using AutoTranslator.Models;
using AutoTranslator.Services.Implementations;
using AutoTranslator.ViewModels.EditorModels;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IPageService
{
    public Task AddPageAsync(Project project, int chapter, int page, Stream stream);
    public Task<bool> MarkTranslatedAsync(Project project, int chapter, int page, Stream stream);
    public string? GetPageImagePath(Project project, int chapterNumber, int pageNumber, PageStatus status);
    public int GetNextPageNumber(Project project, int chapterNumber);
    public List<PageInfo> GetPages(Project project, int chapterNumber);

}
