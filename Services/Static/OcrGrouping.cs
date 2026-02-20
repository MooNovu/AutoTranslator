using AutoTranslator.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace AutoTranslator.Services.Static;


public static class OcrGrouping
{
    public static List<MergedBlock> MergeTextBlocks(
        List<OcrWord> blocks,
        int verticalThreshold = 20,
        int horizontalThreshold = 30,
        double minConfidence = 0.8)
    {
        if (blocks == null || blocks.Count == 0) return [];


        var validBlocks = blocks.Where(b => b.Confidence >= minConfidence).ToList();

        var sorted = validBlocks.OrderBy(b => b.Y).ThenBy(b => b.X).ToList();
        var merged = new List<MergedBlock>();
        var used = new HashSet<int>();

        for (int i = 0; i < sorted.Count; i++)
        {
            if (used.Contains(i)) continue;

            var current = sorted[i];
            var group = new List<OcrWord> { current };

            int x1 = current.X;
            int y1 = current.Y;
            int x2 = current.X + current.Width;
            int y2 = current.Y + current.Height;

            for (int j = i + 1; j < sorted.Count; j++)
            {
                if (used.Contains(j)) continue;

                var next = sorted[j];

                int verticalGap = next.Y - y2;

                double currentCenterX = (x1 + x2) / 2.0;
                double nextCenterX = next.X + next.Width / 2.0;

                bool horizontalOverlap = Math.Min(x2, next.X + next.Width) - Math.Max(x1, next.X) > 0;

                bool alignedHorizontal = Math.Abs(currentCenterX - nextCenterX) <
                    Math.Max(x2 - x1, next.Width) * 0.5;

                if (verticalGap < verticalThreshold && (horizontalOverlap || alignedHorizontal))
                {
                    group.Add(next);
                    used.Add(j);

                    x1 = Math.Min(x1, next.X);
                    y1 = Math.Min(y1, next.Y);
                    x2 = Math.Max(x2, next.X + next.Width);
                    y2 = Math.Max(y2, next.Y + next.Height);
                }
            }

            group = [.. group.OrderBy(b => b.Y).ThenBy(b => b.X)];

            string mergedText = string.Join(" ", group.Select(b => b.Text.Trim()));


            double avgConfidence = group.Average(b => b.Confidence);

            merged.Add(new MergedBlock
            {
                Text = mergedText,
                Bounds = new Rectangle(
                    x1, y1,
                    x2 - x1,
                    y2 - y1
                ),
                Confidence = avgConfidence,
                OriginalBlocks = [.. group]
            });
        }

        return merged;
    }
}