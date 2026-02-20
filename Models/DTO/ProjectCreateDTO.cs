using AutoTranslator.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTranslator.Models.DTO;

public class ProjectCreateDTO(string name, string description, Language sourceLanguage, Language targetLanguage)
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public Language SourceLanguage { get; set; } = sourceLanguage;
    public Language TargetLanguage { get; set; } = targetLanguage;

}
