using AutoTranslator.Models.Enums;

namespace AutoTranslator.Models.DTO;
public record ProjectCreateDTO(string Name, string Description, Language SourceLanguage, Language TargetLanguage, string? FontFileName = null);
