using AutoTranslator.Models;
using AutoTranslator.Models.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoTranslator.Services.Static;

public static class ProjectHelper
{
    public const string ProjectFileExtension = ".atproj";
    public const string OriginalImageSuffix = "_orig";
    public const string TranslatedImageSuffix = "_done";
    public const string CleanedImageSuffix = "_clean";
    public const string ImageExtension = ".png";
    public const string ChapterSeparator = "_";

    public static string SanitizeFileName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    public static string EnsureUniqueFolder(string basePath)
    {
        var path = basePath;
        int counter = 1;

        while (Directory.Exists(path))
            path = $"{basePath}_{counter++}";

        return path;
    }
    public static string FormatChapterFolder(int chapterNumber, string chapterName)
        => $"{chapterNumber:D3}{ChapterSeparator}{chapterName}";

    public static string FormatPageFile(int pageNumber, bool translated)
        => $"{pageNumber:D3}{(translated ? TranslatedImageSuffix : OriginalImageSuffix)}{ImageExtension}";

    public static string FromOrigToCleaned(string origFileName) =>
        origFileName.Replace(OriginalImageSuffix, CleanedImageSuffix);
    public static string FromOrigToDone(string origFileName) =>
        origFileName.Replace(OriginalImageSuffix, TranslatedImageSuffix);

    public static IEnumerable<string> GetOllamaModels()
    {
        return
            [
            "gpt-oss:120b",
            "gpt-oss:20b",
            "gemma3:27b",
            "gemma3:12b", 
            "gemma3:4b",
            "gemma3:1b",

            "deepseek-r1:8b",

            "qwen3:30b",
            "qwen3:8b",
            "qwen3:4b",
            ];
    }

    public static string SystemPrompt(Language source, Language target)
    {
        return
            $"""
            You are an expert manga/comic translator with a deep understanding of visual storytelling and dialogue flow.

            Translate the following text from {source.GetDisplayName()} to {target.GetDisplayName()}.

            CRITICAL CONTEXT:
            1.  **Speech Balloons & Narrative Flow:** The input text represents dialogue or narration from sequential panels (speech bubbles/boxes). They are separated by "&&". These bubbles are connected. Your translation must maintain the conversational flow, honorifics, and grammatical cases (if applicable in the target language) across these segments. Avoid literal translations if they break the natural connection between bubbles.
            2.  **Tone:** Match the original character's voice (e.g., polite, rough, childish, formal).
            3.  **Locations:** The "&&" delimiter must remain exactly in the same positions as in the original. Do not add or remove any.

            OUTPUT FORMAT (Strictly follow this structure):

            [TranslatedText]
            <Insert your connected, natural-sounding translation here. Ensure the "&&" delimiters are preserved in the correct sequence.>

            [DictionaryUpdate]
            <Insert only proper nouns (names, places, unique terms) and their chosen translation here. Format as "Original -> Translation". If there are no new or important names/terms to register, leave this section completely blank. Do not add common words.>

            Key Instructions for Dictionary:
            - ONLY include names, locations, or recurring fictional terms.
            - If a name appears that has already been established, you do not need to repeat it unless you are changing the translation.
            - If there are no names or specific terms in this segment, output NOTHING after [DictionaryUpdate] (an empty line is fine).

            Key Instructions for Translation:
            - Prioritize the narrative connection between the "&&" separated segments.
            - Adjust for target language grammar (cases, pronouns) based on the relationship implied across the text blocks.
            """;
    }

    public static string UserPrompt(List<MergedBlock> blocks)
    {
        var originalText = string.Join("&&", blocks.Select(b => b.Text));

        return
            $"""
            [Dictionary]

            [OriginalText]
            {originalText}
            """;
    }
}