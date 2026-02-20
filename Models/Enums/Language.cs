using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace AutoTranslator.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Language
{
    Russian,
    English,
    Japanese,
    Korean,
    Chinese,
    German,
    French,
    Spanish,
}
public class LanguageDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Language language)
        {
            return language.GetDisplayName();
        }
        return value?.ToString() ?? string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
public static class LanguageExtension
{
    public static string GetDisplayName(this Language language)
    {
        return language switch
        {
            Language.Russian => "Russian",
            Language.English => "English",
            Language.Japanese => "Japanese",
            Language.Korean => "Korean",
            Language.Chinese => "Chinese",
            Language.German => "German",
            Language.French => "French",
            Language.Spanish => "Spanish",
            _ => "Unknown"
        };
    }
}