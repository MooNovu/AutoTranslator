using AutoTranslator.Models.DTO;
using System;
using System.Collections.Generic;

namespace AutoTranslator.Services.Static;

public static class LlmResponseParser
{
    public static LlmResponse Parse(string raw)
    {
        var result = new LlmResponse();

        if (string.IsNullOrWhiteSpace(raw))
            return result;

        string translatedSection = ExtractSection(raw, "TranslatedText");
        string dictionarySection = ExtractSection(raw, "DictionaryUpdate");

        // --- Parse TranslatedText ---
        if (!string.IsNullOrWhiteSpace(translatedSection))
        {
            List<string> translations = [.. translatedSection.Split("&&", StringSplitOptions.TrimEntries)];
            List<string> res = [];
            foreach (string translation in translations) 
            {
                if (string.IsNullOrEmpty(translation)) continue;
                res.Add(translation);
            }

            result.TranslatedBlocks = res;
        }

        // --- Parse DictionaryUpdate ---
        if (!string.IsNullOrWhiteSpace(dictionarySection))
        {
            var lines = dictionarySection
                .Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split('-', 2, StringSplitOptions.TrimEntries);

                if (parts.Length == 2)
                    result.DictionaryUpdate[parts[0]] = parts[1];
            }
        }

        return result;
    }

    private static string ExtractSection(string text, string sectionName)
    {
        var startTag = $"[{sectionName}]";
        var startIndex = text.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);

        if (startIndex == -1)
            return "";

        startIndex += startTag.Length;

        var endIndex = text.IndexOf("[", startIndex);

        if (endIndex == -1)
            endIndex = text.Length;

        return text[startIndex..endIndex].Trim();
    }
}