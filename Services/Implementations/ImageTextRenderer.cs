using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoTranslator.Services.Implementations;

public class ImageTextRenderer(ISettingsService settings, ILogger<ImageTextRenderer> logger) : IImageTextRenderer
{
    private readonly ILogger<ImageTextRenderer> _logger = logger;
    private readonly string _fontsFolder = Path.Combine(settings.Settings.AssetFolder, "Fonts");

    public void DrawTextBlocks(
        string imagePath,
        string outputPath,
        List<MergedBlock> blocks, 
        string? fontName = null)
    {

        _logger.LogInformation("Starting text rendering for image: {ImagePath}, output: {OutputPath}, blocks count: {BlockCount}", 
            imagePath, outputPath, blocks?.Count ?? 0);

        if (blocks == null || blocks.Count == 0)
        {
            _logger.LogWarning("No text blocks to render");
            return;
        }

        FontCollection fontCollection = new();
        FontFamily fallbackFont = SystemFonts.Get("Arial");
        FontFamily actualFontFamily;

        if (!string.IsNullOrEmpty(fontName))
        {
            string fontPath = Path.Combine(_fontsFolder, fontName);

            try
            {
                if (!File.Exists(fontPath)) throw new FileNotFoundException("Font not found", fontPath);

                fontCollection.Add(fontPath);

                _logger.LogInformation("Using font {FontPath}", fontPath);
                actualFontFamily = fontCollection.Families.First();
            }
            catch (System.Exception)
            {
                _logger.LogError("Font file not found at {FontPath}, using fallback font", fontPath);
                actualFontFamily = fallbackFont;
            }
        }
        else
        {
            _logger.LogInformation("Using default font");
            actualFontFamily = fallbackFont;
        }


        try
        {
            if (!File.Exists(imagePath))
            {
                _logger.LogError("Source image not found: {ImagePath}", imagePath);
                throw new FileNotFoundException("Source image not found", imagePath);
            }

            using var image = Image.Load(imagePath);

            _logger.LogDebug("Image loaded successfully. Size: {Width}x{Height}", image.Width, image.Height);

            image.Mutate(ctx =>
            {
                int renderedCount = 0;
                foreach (var block in blocks)
                {
                    if (block == null || string.IsNullOrWhiteSpace(block.Text))
                    {
                        _logger.LogDebug("Skipping empty block at index {BlockIndex}", renderedCount);
                        continue;
                    }

                    _logger.LogDebug("Drawing block {BlockIndex}: '{TextPreview}' at position [{X},{Y}] size [{Width}x{Height}]",
                        renderedCount,
                        block.Text.Length > 30 ? string.Concat(block.Text.AsSpan(0, 27), "...") : block.Text,
                        block.Bounds.X, block.Bounds.Y, block.Bounds.Width, block.Bounds.Height);

                    DrawTextInRectangle(ctx, block.Text, block.Bounds, actualFontFamily);
                    renderedCount++;
                }
                _logger.LogDebug("Successfully rendered {RenderedCount} of {TotalCount} text blocks", renderedCount, blocks.Count);
            });

            image.SaveAsPng(outputPath);

            _logger.LogInformation("Text rendering completed successfully. Output saved to: {OutputPath}", outputPath);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error during text rendering for image: {ImagePath}", imagePath);
            throw;
        }
    }

    private void DrawTextInRectangle(
        IImageProcessingContext image,
        string text,
        Rectangle bounds, 
        FontFamily fontFamily)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        float fontSize = bounds.Height;
        RichTextOptions textOptions;
        Font font;


        FontRectangle textSize;
        do
        {
            fontSize -= 1f;

            font = new Font(fontFamily, fontSize, FontStyle.Bold);

            textOptions = new RichTextOptions(font)
            {
                WrappingLength = bounds.Width,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                WordBreaking = WordBreaking.Standard
            };

            textSize = TextMeasurer.MeasureSize(text, textOptions);
        } 
        while ((textSize.Height > bounds.Height ||
                  textSize.Width > bounds.Width) &&
                  fontSize > 6);

        font = new Font(fontFamily, fontSize, FontStyle.Bold);

        textOptions = new RichTextOptions(font)
        {
            WrappingLength = bounds.Width,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            WordBreaking = WordBreaking.Standard,
            LineSpacing = 1.2f,
            Origin = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + bounds.Height / 2f)
        };

        var color = Color.Black;

        image.DrawText(textOptions, text, color);
    }
}

