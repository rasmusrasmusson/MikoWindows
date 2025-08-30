using MikoMe.Helpers;
using MikoMe.Models;
using MikoMe.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MikoMe.ViewModels
{
    public class SessionViewModel : ObservableObject
    {
        // NOTE: TtsService is static now, so we do NOT new it here.

        private List<Card> _queue = new();
        private int _index = -1;

        public CardDirection Direction { get; private set; } = CardDirection.ZhToEn;

        private string _promptBig = string.Empty;
        public string PromptBig { get => _promptBig; set => SetProperty(ref _promptBig, value); }

        private string _promptSmall = string.Empty;
        public string PromptSmall { get => _promptSmall; set => SetProperty(ref _promptSmall, value); }

        private string _answerText = string.Empty;
        public string AnswerText { get => _answerText; set => SetProperty(ref _answerText, value); }

        private bool _isAnswerShown;
        public bool IsAnswerShown
        {
            get => _isAnswerShown;
            set { SetProperty(ref _isAnswerShown, value); OnVisibilityChanged?.Invoke(); }
        }

        private bool _showPinyin = true;
        public bool ShowPinyin
        {
            get => _showPinyin;
            set { SetProperty(ref _showPinyin, value); OnVisibilityChanged?.Invoke(); }
        }

        private string _progressText = string.Empty;
        public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }

        public event Action? OnVisibilityChanged;

        public ICommand ShowCommand => new RelayCommand(() =>
        {
            if (!IsAnswerShown) IsAnswerShown = true;
            else _ = GradeAsync(true);
        });

        public ICommand PassCommand => new RelayCommand(() => _ = GradeAsync(true));
        public ICommand FailCommand => new RelayCommand(() => _ = GradeAsync(false));
        public ICommand SpeakCommand => new RelayCommand(() => _ = SpeakAsync());
        public ICommand TogglePinyinCommand => new RelayCommand(() => ShowPinyin = !ShowPinyin);

        public async Task InitAsync(CardDirection direction)
        {
            Direction = direction;
            await BuildQueueAsync();
            _index = -1;
            await NextAsync();
        }

        private async Task BuildQueueAsync()
        {
            var now = DateTime.UtcNow;
            var db = DatabaseService.Context;

            _queue = await db.Cards
                .Include(c => c.Word)
                .Where(c => c.Direction == Direction && c.DueAtUtc <= now)
                .OrderBy(c => c.DueAtUtc)
                .Take(100)
                .ToListAsync();

            if (_queue.Count == 0)
            {
                _queue = await db.Cards
                    .Include(c => c.Word)
                    .Where(c => c.Direction == Direction)
                    .OrderBy(c => c.DueAtUtc)
                    .Take(10)
                    .ToListAsync();
            }

            UpdateProgress();
        }

        private void UpdateProgress()
        {
            ProgressText = _queue.Count == 0
                ? "No cards due"
                : $"{Math.Clamp(_index + 1, 0, _queue.Count)}/{_queue.Count} due";
        }

        private async Task SpeakAsync()
        {
            var card = Current;
            if (card?.Word == null) return;

            var text = card.Word.Hanzi;

            // TtsService is static → call it directly
            await TtsService.SpeakAsync(text, "zh-CN");
        }

        public Card? Current => (_index >= 0 && _index < _queue.Count) ? _queue[_index] : null;

        private Task NextAsync()
        {
            _index++;
            UpdateProgress();

            if (Current == null)
            {
                PromptBig = "All done for now 🎉";
                PromptSmall = "";
                AnswerText = "No more cards due.";
                IsAnswerShown = true;
                return Task.CompletedTask;
            }

            var w = Current!.Word;
            if (Direction == CardDirection.ZhToEn)
            {
                PromptBig = w.Hanzi;
                PromptSmall = w.Pinyin;
                AnswerText = w.English;
            }
            else
            {
                PromptBig = w.English;
                PromptSmall = "";
                AnswerText = $"{w.Hanzi}  ({w.Pinyin})";
            }

            IsAnswerShown = false;
            return Task.CompletedTask;
        }

        private async Task GradeAsync(bool known)
        {
            var card = Current;
            if (card == null) return;

            var now = DateTime.UtcNow;
            var db = DatabaseService.Context;
            var prevI = card.IntervalDays;
            var prevE = card.Ease;

            Scheduler.ApplyReview(card, known, now);

            var elapsed = 0;
            if (card.LastReviewedAtUtc.HasValue)
                elapsed = (int)Math.Max(0, (now - card.LastReviewedAtUtc.Value).TotalDays);

            db.ReviewLogs.Add(new ReviewLog
            {
                CardId = card.Id,
                ReviewedAtUtc = now,
                Grade = known ? 4 : 2,
                PrevInterval = prevI,
                NextInterval = card.IntervalDays,
                PrevEase = prevE,
                NextEase = card.Ease,
                ElapsedDays = elapsed
            });

            await db.SaveChangesAsync();
            await NextAsync();
        }
    }
}
