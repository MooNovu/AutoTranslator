using System.Collections.Generic;
using SixLabors.ImageSharp;

namespace AutoTranslator.Models;

public class OcrWord
{
    public string Text { get; set; } = string.Empty;

    //Bounding Box
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public double Confidence { get; set; }
}

public class MergedBlock
{
    public string Text { get; set; } = string.Empty;
    public Rectangle Bounds { get; set; }
    public double Confidence { get; set; }
    public List<OcrWord> OriginalBlocks { get; set; } = [];
}