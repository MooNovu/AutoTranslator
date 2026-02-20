using AutoTranslator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface ITextEraseService
{
    Task<string> EraseTextAsync(string imagePath, List<MergedBlock> blocks);
}
