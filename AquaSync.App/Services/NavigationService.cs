using AquaSync.App.Contracts.Services;
using AquaSync.App.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace AquaSync.App.Services;

/// <summary>
/// Implements Frame-based navigation for the ShellPage's content area.
/// </summary>
public sealed class NavigationService : INavigationService
{
    private readonly IPageService _pageService;
    private Frame? _frame;
    private object? _lastParameterUsed;

    public NavigationService(IPageService pageService)
    {
        _pageService = pageService;
    }

    public event NavigatedEventHandler? Navigated;

    public bool CanGoBack => Frame?.CanGoBack ?? false;

    public Frame? Frame
    {
        get => _frame;
        set
        {
            UnregisterFrameEvents();
            _frame = value;
            RegisterFrameEvents();
        }
    }

    public bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false)
    {
        var pageType = _pageService.GetPageType(pageKey);

        if (Frame is null)
        {
            return false;
        }

        if (Frame.Content?.GetType() == pageType
            && (parameter is null || parameter.Equals(_lastParameterUsed)))
        {
            return false;
        }

        Frame.Tag = clearNavigation;
        var navigated = Frame.Navigate(pageType, parameter);

        if (navigated)
        {
            _lastParameterUsed = parameter;
        }

        return navigated;
    }

    public bool GoBack()
    {
        if (!CanGoBack)
        {
            return false;
        }

        var vmBeforeNavigation = _frame?.GetPageViewModel();
        _frame?.GoBack();

        if (vmBeforeNavigation is INavigationAware navigationAware)
        {
            navigationAware.OnNavigatedFrom();
        }

        return true;
    }

    private void RegisterFrameEvents()
    {
        if (_frame is not null)
        {
            _frame.Navigated += OnNavigated;
        }
    }

    private void UnregisterFrameEvents()
    {
        if (_frame is not null)
        {
            _frame.Navigated -= OnNavigated;
        }
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
        if (sender is Frame frame)
        {
            var clearNavigation = frame.Tag is true;
            if (clearNavigation)
            {
                frame.BackStack.Clear();
            }

            if (frame.GetPageViewModel() is INavigationAware navigationAware)
            {
                navigationAware.OnNavigatedTo(e.Parameter);
            }

            Navigated?.Invoke(sender, e);
        }
    }
}
