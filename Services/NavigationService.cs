using Microsoft.UI.Xaml.Controls;

namespace MikoMe.Services
{
    /// <summary>
    /// Global navigation helper for pages to navigate via the main Frame.
    /// </summary>
    public static class NavigationService
    {
        public static Frame? RootFrame { get; set; }

        public static bool Navigate<TPage>(object? parameter = null) where TPage : class
            => RootFrame?.Navigate(typeof(TPage), parameter) ?? false;
    }
}
