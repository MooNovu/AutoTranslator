using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTranslator.ViewModels.Base;

/// <summary>
/// Базовый класс для всех ViewModel
/// </summary>
public abstract partial class ViewModelBase(IServiceProvider serviceProvider) : ObservableObject
{
    /// <summary>
    /// Заголовок UI (Отображается в UI)
    /// </summary>
    [ObservableProperty]
    private string? _title;

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Сообщение статуса, для отображения в UI
    /// </summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>
    /// Провайдер сервисов для доступа к зависимостям
    /// </summary>
    protected readonly IServiceProvider ServiceProvider = serviceProvider;

    /// <summary>
    /// Вызывается при переходе на эту ViewModel
    /// </summary>
    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;

    /// <summary>
    /// Вызывается при уходе с этой ViewModel
    /// </summary>
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;

    private CancellationTokenSource? _messageCancellationTokenSource;

    protected void ShowMessage(string message)
    {
        _messageCancellationTokenSource?.Cancel();
        _messageCancellationTokenSource?.Dispose();

        _messageCancellationTokenSource = new CancellationTokenSource();
        var token = _messageCancellationTokenSource.Token;

        StatusMessage = message;

        Task.Delay(3000, token).ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully && !token.IsCancellationRequested)
            {
                StatusMessage = null;
            }
        }, token);
    }
}
