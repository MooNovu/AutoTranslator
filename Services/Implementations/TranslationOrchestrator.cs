using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Exception;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class TranslationOrchestrator(IOcrServiceFactory ocrFactory, ILlmServiceFactory llmFactory) : ITranslationOrchestrator
{
    private readonly IOcrServiceFactory _ocrFactory = ocrFactory;
    private readonly ILlmServiceFactory _llmFactory = llmFactory;
    public async Task<List<MergedBlock>> RunOcrAsync(string imagePath)
    {
        IOcrService ocrService = _ocrFactory.Create();

        var words = await ocrService.RecognizeAsync(imagePath);

        var realWords = words
            .Where(w => !string.IsNullOrWhiteSpace(w.Text))
            .ToList();

        return OcrGrouping.MergeTextBlocks(realWords);
    }

    public async Task<List<MergedBlock>> TranslateBlocksAsync(List<MergedBlock> blocks, Language fromLang, Language toLang)
    {
        ILlmService llmService = _llmFactory.Create();

        var request = new LlmRequest
        {
            SystemPrompt = ProjectHelper.SystemPrompt(fromLang, toLang),
            UserPrompt = ProjectHelper.UserPrompt(blocks)
        };

        LlmResponse response = await llmService.TranslateAsync(request);

        if (response.TranslatedBlocks.Count < blocks.Count) throw new LlmException("LLM вернул неполный перевод.", true);

        return MapTranslatedBlocks(blocks, response.TranslatedBlocks);
    }

    private static List<MergedBlock> MapTranslatedBlocks(List<MergedBlock> blocks, List<string> translations)
    {
        List<MergedBlock> result = [];
        for (int i = 0; i < blocks.Count; i++)
        {
            MergedBlock block = new()
            {
                Text = translations[i],
                Bounds = blocks[i].Bounds,
                Confidence = blocks[i].Confidence,
                OriginalBlocks = blocks[i].OriginalBlocks,
            };
            result.Add(block);
        }
        return result;
    }
}
