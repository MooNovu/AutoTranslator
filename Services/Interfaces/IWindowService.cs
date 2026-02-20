using AutoTranslator.Models;
using AutoTranslator.ViewModels.Base;
using Avalonia.Controls;
using System.Threading.Tasks;

namespace AutoTranslator.Services.Interfaces;

public interface IWindowService
{
    public void OpenEditor(Project project);
    public Task<Project?> OpenNewProjectDialogAsync();
    public void CloseCurrentWindow(object viewModel);
    public Task<TResult?> ShowDialogAsync<TWindow, TViewModel, TResult>(TViewModel vm) where TWindow : Window, new();
}