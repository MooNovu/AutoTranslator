using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoTranslator.ViewModels.EditorModels;

public partial class PageInfo : ObservableObject
{
    public int Number { get; }

    [ObservableProperty]
    private bool _isTranslated;

    public Bitmap? OriginalImage { get; set; }
    public Bitmap? TranslatedImage { get; set; }

    public string DisplayName =>
        $"{Number:D3} {(IsTranslated ? "✓" : "⏳")}";

    public PageInfo(int number, bool isTranslated)
    {
        Number = number;
        IsTranslated = isTranslated;
    }
}