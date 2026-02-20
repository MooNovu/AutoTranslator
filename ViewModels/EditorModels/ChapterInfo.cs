using CommunityToolkit.Mvvm.ComponentModel;

namespace AutoTranslator.ViewModels.EditorModels;

public partial class ChapterInfo : ObservableObject
{
    public int Number { get; }
    public string Name { get; }

    [ObservableProperty]
    private int _pageCount;

    public string DisplayName => $"{Number:D3} - {Name} ({PageCount})";

    public ChapterInfo(int number, string name, int pageCount)
    {
        Number = number;
        Name = name;
        PageCount = pageCount;
    }
}
