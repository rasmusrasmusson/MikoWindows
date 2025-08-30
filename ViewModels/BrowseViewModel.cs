
using MikoMe.Helpers;
using MikoMe.Models;
using MikoMe.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MikoMe.ViewModels;

public class BrowseViewModel : ObservableObject
{
    public ObservableCollection<Word> Items { get; } = new();

    public async Task LoadAsync()
    {
        Items.Clear();
        var all = await DatabaseService.Context.Words.AsNoTracking().ToListAsync();
        foreach (var w in all) Items.Add(w);
    }
}
