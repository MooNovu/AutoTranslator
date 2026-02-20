using AutoTranslator.ViewModels.Base;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using Avalonia;
using System.Linq;

namespace AutoTranslator.ViewModels.Pages;

public partial class CreateChapterDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _chapterName = string.Empty;

    public bool IsConfirmed { get; private set; }

    [RelayCommand]
    private void Confirm()
    {
        Debug.WriteLine($"Confirm executed, ChapterName: {ChapterName}");
        IsConfirmed = true;

        // Находим окно и закрываем его с результатом
        var window = GetWindow();
        window?.Close(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        Debug.WriteLine("Cancel executed");
        IsConfirmed = false;

        var window = GetWindow();
        window?.Close(false);
    }

    private Window? GetWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.FirstOrDefault(w => w.DataContext == this);
        }
        return null;
    }
}
