using AutoTranslator.Models;
using AutoTranslator.ViewModels.EditorModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IChapterService
{
    public Task CreateChapterAsync(Project project, int chapterNumber, string chapterName);
    public Task DeleteChapterAsync(Project project, int chapterNumber);
    public List<ChapterInfo> GetAllChapters(Project project);
}
