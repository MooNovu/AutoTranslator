using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoTranslator.Services.Implementations;

public class ImageTextRenderer : IImageTextRenderer
{
    private readonly FontCollection _fontCollection = new();

    public ImageTextRenderer(IServiceProvider sp)
    {
        ISettingsService settings = sp.GetRequiredService<ISettingsService>();

        var fontPath = Path.Combine(
            settings.Settings.AssetFolder,
            "Fonts",
            "AnimeAce2.ttf");

        if (!File.Exists(fontPath))
            throw new FileNotFoundException("Font not found", fontPath);

        _fontCollection.Add(fontPath);
    }

    public void DrawTextBlocks(
        string imagePath,
        string outputPath,
        List<MergedBlock> blocks)
    {
        using var image = Image.Load(imagePath);

        image.Mutate(ctx => {
            foreach (var block in blocks)
            {
                DrawTextInRectangle(ctx, block.Text, block.Bounds);
            }
        });

        image.SaveAsPng(outputPath);
    }

    private void DrawTextInRectangle(
        IImageProcessingContext image,
        string text,
        Rectangle bounds)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        float fontSize = bounds.Height;
        RichTextOptions textOptions;
        Font font;
        FontFamily fallbackFont = SystemFonts.Get("Arial");
        FontFamily actualFontFamily;
        try
        {
            actualFontFamily = _fontCollection.Families.First();
        }
        catch (System.Exception)
        {
            actualFontFamily = fallbackFont;
        }

        FontRectangle textSize;
        do
        {
            fontSize -= 1f;

            font = new Font(actualFontFamily, fontSize, FontStyle.Bold);

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

        font = new Font(actualFontFamily, fontSize, FontStyle.Bold);

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

