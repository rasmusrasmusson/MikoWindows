using MikoMe.Models;
using MikoMe.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace MikoMe.Views
{
    public sealed partial class EditWordPage : Page
    {
        private Card? _card;

        public EditWordPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int cardId)
            {
                _card = DatabaseService.Context.Cards
                    .Include(c => c.Word)
                    .FirstOrDefault(c => c.Id == cardId);

                if (_card?.Word != null)
                {
                    HanziTextBox.Text = _card.Word.Hanzi;
                    PinyinTextBox.Text = _card.Word.Pinyin;
                    EnglishTextBox.Text = _card.Word.English;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_card?.Word != null)
            {
                _card.Word.Hanzi = HanziTextBox.Text;
                _card.Word.Pinyin = PinyinTextBox.Text;
                _card.Word.English = EnglishTextBox.Text;

                DatabaseService.Context.SaveChanges();

                // Back to Browse
                Services.NavigationService.Navigate<BrowsePage>();
                ShowConfirmation("Vocabulary updated successfully.");
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_card != null)
            {
                DatabaseService.Context.Cards.Remove(_card);
                if (_card.Word != null)
                {
                    DatabaseService.Context.Words.Remove(_card.Word);
                }

                DatabaseService.Context.SaveChanges();

                // Back to Browse
                Services.NavigationService.Navigate<BrowsePage>();
                ShowConfirmation("Vocabulary deleted successfully.");
            }
        }

        private async void ShowConfirmation(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirmation",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
