using AutoTranslator.Models;
using AutoTranslator.Services.Interfaces;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using AutoTranslator.ViewModels.Pages;

namespace AutoTranslator.Services.Implementations;

public class WindowService(IServiceProvider serviceProvider) : IWindowService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public void CloseCurrentWindow(object viewModel)
    {
        var window = GetWindowByViewModel(viewModel);
        window?.Close();
    }
    private Window? GetWindowByViewModel(object viewModel)
    {
        return Application.Current?
            .ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
            ? desktop.Windows.FirstOrDefault(w => w.DataContext == viewModel)
            : null;
    }

    public void OpenEditor(Project project)
    {
        var vm = _serviceProvider.GetRequiredService<ProjectEditorViewModel>();
        vm.LoadProject(project);

        var window = new ProjectEditorView
        {
            DataContext = vm,
        };

        window.Show();
    }
    public async Task<Project?> OpenNewProjectDialogAsync()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop) return null;

        var owner = desktop.MainWindow;
        if (owner == null) return null;

        var vm = _serviceProvider.GetRequiredService<CreateProjectDialogViewModel>();

        var window = new CreateProjectDialog
        {
            DataContext = vm
        };

        Project? result = null;

        vm.DialogCompleted += project =>
        {
            result = project;
            window.Close();
        };

        await window.ShowDialog(owner);

        return result;
    }

    public async Task<TResult?> ShowDialogAsync<TWindow, TViewModel, TResult>(TViewModel vm) where TWindow : Window, new()
    {
        if (Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime desktop)
            return default;

        var owner = desktop.Windows.FirstOrDefault(w => w.IsVisible);
        if (owner == null)
            return default;

        var window = new TWindow
        {
            DataContext = vm
        };

        TResult? result = default;

        if (vm is ErrorDialogViewModel errorVm)
        {
            errorVm.DialogClosed += r =>
            {
                result = (TResult)(object)r!;
                window.Close();
            };
        }

        await window.ShowDialog(owner);

        return result;
    }
}