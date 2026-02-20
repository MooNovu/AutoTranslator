using AutoTranslator.ViewModels.Pages;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace AutoTranslator;


public partial class ProjectEditorView : Window
{
    public ProjectEditorView()
    {
        InitializeComponent();
    }

    private void OnPageDragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
            e.DragEffects = DragDropEffects.Copy;
        else
            e.DragEffects = DragDropEffects.None;
    }

    private async void OnPageDrop(object? sender, DragEventArgs e)
    {
        if (!e.DataTransfer.Contains(DataFormat.File))
            return;

        var files = e.DataTransfer.TryGetFiles();
        if (files == null)
            return;

        foreach (var file in files)
        {
            if (file.Path.LocalPath is { } path &&
                DataContext is ProjectEditorViewModel vm)
            {
                await vm.AddPageFromFileAsync(path);
            }
        }
    }
}