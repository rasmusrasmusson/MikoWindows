using System;

namespace MikoMe.Models
{
    public class Card
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public Word Word { get; set; } = default!;
        public CardDirection Direction { get; set; }
        public DateTime DueAtUtc { get; set; } = DateTime.UtcNow;
        public int IntervalDays { get; set; } = 0;
        public double Ease { get; set; } = 2.5;
        public int Reps { get; set; } = 0;
        public int Lapses { get; set; } = 0;
        public string State { get; set; } = "New";
        public DateTime? LastReviewedAtUtc { get; set; }
    }
}
