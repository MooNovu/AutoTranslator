using AutoTranslator.Models;
using AutoTranslator.Models.DTO;
using AutoTranslator.Models.Enums;
using AutoTranslator.Services.Exception;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class TranslationOrchestrator(IOcrServiceFactory ocrFactory, ILlmServiceFactory llmFactory, ILogger<TranslationOrchestrator> logger) : ITranslationOrchestrator
{
    private readonly IOcrServiceFactory _ocrFactory = ocrFactory;
    private readonly ILlmServiceFactory _llmFactory = llmFactory;
    private readonly ILogger<TranslationOrchestrator> _logger = logger;
    public async Task<List<MergedBlock>> RunOcrAsync(string imagePath)
    {
        IOcrService ocrService = _ocrFactory.Create();

        _logger.LogInformation("Starting OCR for {ImagePath}", imagePath);

        try
        {
            var words = await ocrService.RecognizeAsync(imagePath);
            _logger.LogDebug("OCR return {Count} words", words.Count);

            var realWords = words
                .Where(w => !string.IsNullOrWhiteSpace(w.Text))
                .ToList();

            List<MergedBlock> result = OcrGrouping.MergeTextBlocks(realWords);

            _logger.LogInformation("OCR completed successfully. Created {BlockCount} merged blocks", result.Count);

            return result;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "OCR Error for {ImagePath}", imagePath);
            throw;
        }
    }

    public async Task<List<MergedBlock>> TranslateBlocksAsync(List<MergedBlock> blocks, Language fromLang, Language toLang)
    {
        ILlmService llmService = _llmFactory.Create();

        _logger.LogInformation("Starting translation of {BlockCount} blocks from {FromLang} to {ToLang}", blocks.Count, fromLang, toLang);

        var request = new LlmRequest
        {
            SystemPrompt = ProjectHelper.SystemPrompt(fromLang, toLang),
            UserPrompt = ProjectHelper.UserPrompt(blocks)
        };

        try
        {
            LlmResponse response = await llmService.TranslateAsync(request);

            if (response.TranslatedBlocks.Count < blocks.Count)
            {
                _logger.LogWarning("LLM returned incomplete translation. Expected {ExpectedCount}, got {ActualCount}", 
                    blocks.Count, response.TranslatedBlocks.Count);

                throw new LlmException("LLM вернул неполный перевод.", true);
            }

            var result = MapTranslatedBlocks(blocks, response.TranslatedBlocks);

            _logger.LogInformation("Translation completed successfully for {BlockCount} blocks", result.Count);

            return result;
        }
        catch (LlmException)
        {
            _logger.LogError("LLM translation failed: incomplete translation");
            throw;
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during translation");
            throw;
        }
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
