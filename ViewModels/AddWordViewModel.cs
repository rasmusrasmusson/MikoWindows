
using MikoMe.Helpers;
using MikoMe.Models;
using MikoMe.Services;
using System;
using System.Threading.Tasks;

namespace MikoMe.ViewModels;

public class AddWordViewModel : ObservableObject
{
    private string _english = string.Empty;
    public string English { get => _english; set => SetProperty(ref _english, value); }

    private string _hanzi = string.Empty;
    public string Hanzi { get => _hanzi; set => SetProperty(ref _hanzi, value); }

    private string _pinyin = string.Empty;
    public string Pinyin { get => _pinyin; set => SetProperty(ref _pinyin, value); }

    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(English) || string.IsNullOrWhiteSpace(Hanzi)) return;

        var db = DatabaseService.Context;
        var word = new Word
        {
            English = English.Trim(),
            Hanzi = Hanzi.Trim(),
            Pinyin = Pinyin.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
        db.Words.Add(word);
        await db.SaveChangesAsync();

        db.Cards.Add(new Card { WordId = word.Id, Direction = CardDirection.ZhToEn, DueAtUtc = DateTime.UtcNow });
        db.Cards.Add(new Card { WordId = word.Id, Direction = CardDirection.EnToZh, DueAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        English = Hanzi = Pinyin = string.Empty;
    }
}
