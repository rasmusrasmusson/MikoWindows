using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MikoMe.Services;   // NavigationService
using MikoMe.Views;
using MikoMe.Models;     // CardDirection
using Microsoft.UI.Windowing; // Added for AppWindow
using Windows.Graphics;         // Needed for WindowId
using WinRT;                    // Needed for WindowNative

namespace MikoMe
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // ✅ Set window title
            this.Title = "Miko Me";

            // ✅ Set window icon (unpackaged)
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.SetIcon("Assets\\app.ico"); // Make sure path matches your icon

            // Expose the main Frame to the app
            NavigationService.RootFrame = ContentHost;

            // Default page
            ContentHost.Navigate(typeof(HomePage));
            NavView.SelectedItem = NavView.MenuItems.OfType<NavigationViewItem>().FirstOrDefault();
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            // Use the item's Tag to decide where to go
            var tag = (args.InvokedItemContainer as NavigationViewItem)?.Tag as string;
            if (string.IsNullOrEmpty(tag)) return;

            switch (tag)
            {
                case "home":
                    ContentHost.Navigate(typeof(HomePage));
                    break;

                case "add":
                    ContentHost.Navigate(typeof(AddWordPage));
                    break;

                case "browse":
                    ContentHost.Navigate(typeof(BrowsePage));
                    break;

                case "session_zh_en":
                    ContentHost.Navigate(typeof(SessionPage), CardDirection.ZhToEn);
                    break;

                case "session_en_zh":
                    ContentHost.Navigate(typeof(SessionPage), CardDirection.EnToZh);
                    break;

                case "translate":
                    ContentHost.Navigate(typeof(TranslatePage));
                    break;

                case "help":
                    ContentHost.Navigate(typeof(HelpPage));
                    break;
            }
        }
    }
}
