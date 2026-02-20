
using System.Collections.Generic;

namespace AutoTranslator.Models.DTO;

public class LlmRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
}
public class LlmResponse
{
    public List<string> TranslatedBlocks { get; set; } = [];
    public Dictionary<string, string> DictionaryUpdate { get; set; } = [];
}
