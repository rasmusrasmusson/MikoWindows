using MikoMe.Models;
using MikoMe.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace MikoMe.Views
{
    public sealed partial class BrowsePage : Page
    {
        public ObservableCollection<Card> Cards { get; } = new();
        public ObservableCollection<Card> FilteredCards { get; } = new();

        public BrowsePage()
        {
            InitializeComponent();
            Loaded += BrowsePage_Loaded;
        }

        private async void BrowsePage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            Cards.Clear();
            FilteredCards.Clear();

            // Load cards with Word navigation property, remove duplicates by WordId
            var items = DatabaseService.Context.Cards
                .Include(c => c.Word)
                .GroupBy(c => c.WordId)
                .Select(g => g.First())
                .ToList();

            foreach (var item in items)
            {
                Cards.Add(item);
                FilteredCards.Add(item);
            }
        }

        // ✅ Real-time search
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.Trim().ToLower();
            FilteredCards.Clear();

            foreach (var card in Cards)
            {
                if (string.IsNullOrEmpty(query) ||
                    (card.Word.Hanzi?.Contains(query) ?? false) ||
                    (card.Word.Pinyin?.ToLower().Contains(query) ?? false) ||
                    (card.Word.English?.ToLower().Contains(query) ?? false))
                {
                    FilteredCards.Add(card);
                }
            }
        }

        // ✅ Sorting
        private void SortOldToNew_Click(object sender, RoutedEventArgs e)
            => Refresh(FilteredCards.OrderBy(c => c.Id).ToList());

        private void SortNewToOld_Click(object sender, RoutedEventArgs e)
            => Refresh(FilteredCards.OrderByDescending(c => c.Id).ToList());

        private void SortHanziAZ_Click(object sender, RoutedEventArgs e)
            => Refresh(FilteredCards.OrderBy(c => c.Word.Hanzi).ToList());

        private void SortHanziZA_Click(object sender, RoutedEventArgs e)
            => Refresh(FilteredCards.OrderByDescending(c => c.Word.Hanzi).ToList());

        private void Refresh(System.Collections.Generic.List<Card> sorted)
        {
            FilteredCards.Clear();
            foreach (var item in sorted)
                FilteredCards.Add(item);
        }

        // ✅ Alternating row background colors
        private void ListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (args.ItemContainer is ListViewItem item)
            {
                int index = sender.IndexFromContainer(item);
                item.Background = index % 2 == 0
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Colors.Transparent);
            }
        }
        private void Vocabulary_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Card card)
            {
                Services.NavigationService.Navigate<EditWordPage>(card.Id);
            }
        }

    }
}
