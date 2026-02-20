using AutoTranslator.Models.DTO;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface ILlmService
{
    Task<LlmResponse> TranslateAsync(LlmRequest request);
}
