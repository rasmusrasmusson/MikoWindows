
using MikoMe.Helpers;
using MikoMe.Models;
using MikoMe.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace MikoMe.ViewModels;

public class HomeViewModel : ObservableObject
{
    private int _dueZhEn;
    public int DueZhEn { get => _dueZhEn; set => SetProperty(ref _dueZhEn, value); }

    private int _dueEnZh;
    public int DueEnZh { get => _dueEnZh; set => SetProperty(ref _dueEnZh, value); }

    public async Task RefreshAsync()
    {
        var now = DateTime.UtcNow;
        DueZhEn = await DatabaseService.Context.Cards.CountAsync(c => c.Direction == CardDirection.ZhToEn && c.DueAtUtc <= now);
        DueEnZh = await DatabaseService.Context.Cards.CountAsync(c => c.Direction == CardDirection.EnToZh && c.DueAtUtc <= now);
    }
}
