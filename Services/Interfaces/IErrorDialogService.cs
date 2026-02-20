using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IErrorDialogService
{
    Task<bool> ShowAsync(string title, string message, bool canRetry);
}
