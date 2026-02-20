using AutoTranslator.Services.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class FolderPickerService : IFolderPicker
{
    public async Task<string?> PickFolderAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return null;

        var folders = await desktop.MainWindow!.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Выберите папку проектов",
                AllowMultiple = false,
                SuggestedStartLocation = null
            });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }
}
