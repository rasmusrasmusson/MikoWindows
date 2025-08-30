using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MikoMe.Services;   // NavigationService
using MikoMe.Models;     // CardDirection

namespace MikoMe.Views
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        // --- Buttons on Home ---

        private void Add_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<AddWordPage>();

        private void Browse_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<BrowsePage>();

        private void StartZhEn_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<SessionPage>(CardDirection.ZhToEn);

        private void StartEnZh_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<SessionPage>(CardDirection.EnToZh);

        private void Translate_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<TranslatePage>();

        private void Help_Click(object sender, RoutedEventArgs e)
            => NavigationService.Navigate<HelpPage>();
    }
}
