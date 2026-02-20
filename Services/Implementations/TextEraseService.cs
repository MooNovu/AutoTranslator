using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using AutoTranslator.Services.Static;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class TextEraseService : ITextEraseService
{
    public async Task<string> EraseTextAsync(string imagePath, List<MergedBlock> blocks)
    {
        using var image = await Image.LoadAsync<Rgba32>(imagePath);

        foreach (var block in blocks)
        {
            var rect = new Rectangle(
                block.Bounds.X,
                block.Bounds.Y,
                block.Bounds.Width,
                block.Bounds.Height);

            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White, rect);
            });
        }

        string outputPath = Path.Combine(Path.GetDirectoryName(imagePath)!, 
            ProjectHelper.FromOrigToCleaned(Path.GetFileName(imagePath)));


        await image.SaveAsync(outputPath);

        return outputPath;
    }
}
