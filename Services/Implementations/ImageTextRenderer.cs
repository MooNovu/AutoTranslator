using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace AutoTranslator.Services.Implementations;

public class ImageTextRenderer : IImageTextRenderer
{
    private readonly PrivateFontCollection _fontCollection = new();
    private readonly FontFamily _fontFamily;
    public ImageTextRenderer(IServiceProvider sp)
    {
        ISettingsService settings = sp.GetRequiredService<ISettingsService>();

        var fontPath = Path.Combine(
            settings.Settings.AssetFolder,
            "Fonts",
            "AnimeAce2.ttf");

        if (!File.Exists(fontPath))
            throw new FileNotFoundException("Font not found", fontPath);

        _fontCollection.AddFontFile(fontPath);
        _fontFamily = _fontCollection.Families[0];
    }

    public void DrawTextBlocks(
        string imagePath,
        string outputPath,
        List<MergedBlock> blocks)
    {
        using var bitmap = new Bitmap(imagePath);
        using var graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        foreach (var block in blocks)
        {
            DrawTextInRectangle(graphics, block.Text, block.Bounds);
        }

        bitmap.Save(outputPath, ImageFormat.Png);
    }

    private void DrawTextInRectangle(
        Graphics graphics,
        string text,
        Rectangle rect)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        float fontSize = rect.Height;
        SizeF textSize;

        do
        {
            fontSize -= 2f;

            using var font = new Font(
                _fontFamily,
                fontSize,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            textSize = graphics.MeasureString(
                text,
                font,
                rect.Width);

        } while ((textSize.Height > rect.Height ||
                  textSize.Width > rect.Width) &&
                  fontSize > 6);

        using var finalFont = new Font(
            _fontFamily,
            fontSize,
            FontStyle.Bold,
            GraphicsUnit.Pixel);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            FormatFlags = StringFormatFlags.LineLimit,
            Trimming = StringTrimming.Word
        };

        // Обводка (чтобы выглядело как манга)
        using var path = new GraphicsPath();
        path.AddString(
            text,
            _fontFamily,
            (int)FontStyle.Bold,
            finalFont.Size,
            rect,
            format);

        graphics.FillPath(Brushes.Black, path);
        graphics.DrawPath(Pens.Black, path);
    }
}

