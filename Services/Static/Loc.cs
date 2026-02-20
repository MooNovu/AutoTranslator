using AutoTranslator.Services.Interfaces;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AutoTranslator.Services.Static;

public class LocExtension(string key) : MarkupExtension
{
    public string Key { get; set; } = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var sp = App.Current!.Services!;
        var localization = sp.GetRequiredService<ILocalizationService>();

        var binding = new ReflectionBindingExtension($"[{Key}]")
        {
            Mode = BindingMode.OneWay,
            Source = localization
        };

        return binding.ProvideValue(serviceProvider);
    }
}
