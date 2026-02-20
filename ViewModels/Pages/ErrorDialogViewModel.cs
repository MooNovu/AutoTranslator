
using AutoTranslator.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AutoTranslator.ViewModels.Pages;

public partial class ErrorDialogViewModel(string title, string message, bool canRetry, IServiceProvider serviceProvider) : ViewModelBase(serviceProvider)
{
    public string TitleText { get; } = title;
    public string Message { get; } = message;
    public bool CanRetry { get; } = canRetry;

    public event Action<bool>? DialogClosed;


    [RelayCommand]
    private void Ok()
    {
        DialogClosed?.Invoke(false);
    }

    [RelayCommand]
    private void Retry()
    {
        DialogClosed?.Invoke(true);
    }
}
