using AutoTranslator.Models;
using AutoTranslator.Models.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface ITranslationOrchestrator
{
    public Task<List<MergedBlock>> RunOcrAsync(string imagePath);
    public Task<List<MergedBlock>> TranslateBlocksAsync(List<MergedBlock> blocks, Language fromLang, Language toLang);
}
