

using AutoTranslator.Models;
using System.Collections.Generic;

namespace AutoTranslator.Services.Interfaces;

public interface IImageTextRenderer
{
    public void DrawTextBlocks(string imagePath, string outputPath, List<MergedBlock> blocks, string? fontName = null);
}
