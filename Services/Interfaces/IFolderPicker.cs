using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IFolderPicker
{
    public Task<string?> PickFolderAsync();
}
