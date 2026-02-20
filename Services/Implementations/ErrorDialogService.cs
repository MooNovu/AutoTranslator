
using AutoTranslator.Services.Interfaces;
using AutoTranslator.ViewModels.Pages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Implementations;

public class ErrorDialogService : IErrorDialogService
{
    private readonly IServiceProvider _sp;
    private readonly IWindowService _windowService;


    public ErrorDialogService(IServiceProvider sp)
    {
        _sp = sp;
        _windowService = _sp.GetRequiredService<IWindowService>();
    }

    public async Task<bool> ShowAsync(string title, string message, bool canRetry)
    {
        var vm = new ErrorDialogViewModel(
            title,
            message,
            canRetry,
            _sp);

        var result = await _windowService
            .ShowDialogAsync<ErrorDialog, ErrorDialogViewModel, bool>(vm);

        return result;
    }
}