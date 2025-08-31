using System;
using Microsoft.EntityFrameworkCore; // for [Index]

namespace MikoMe.Models
{
    [Index(nameof(FsrsDueUtc))] // speed up "fetch due cards" queries
    public class Card
    {
        public int Id { get; set; }
        public int WordId { get; set; }
        public Word Word { get; set; } = default!;
        public CardDirection Direction { get; set; }

        // ------- Legacy SM-2-ish fields (kept for back-compat; don't use with FSRS) -------
        [Obsolete("Legacy field. Use FsrsDueUtc instead.")]
        public DateTime DueAtUtc { get; set; } = DateTime.UtcNow;

        [Obsolete("Legacy field. FSRS computes intervals internally.")]
        public int IntervalDays { get; set; } = 0;

        [Obsolete("Legacy field. FSRS uses Difficulty (1..10) instead.")]
        public double Ease { get; set; } = 2.5;

        [Obsolete("Legacy field. Use FsrsReps/FsrsLapses.")]
        public int Reps { get; set; } = 0;

        [Obsolete("Legacy field. Use FsrsLapses.")]
        public int Lapses { get; set; } = 0;

        [Obsolete("Legacy field. FSRS doesn't use SM-2 state strings.")]
        public string State { get; set; } = "New";

        [Obsolete("Legacy field. Use FsrsLastReviewUtc.")]
        public DateTime? LastReviewedAtUtc { get; set; }

        // ----------------------------- FSRS state -----------------------------
        public double? FsrsStability { get; set; }      // S (days)
        public double? FsrsDifficulty { get; set; }      // D [1..10]
        public bool FsrsIsNew { get; set; } = true;
        public int FsrsReps { get; set; }
        public int FsrsLapses { get; set; }
        public DateTime? FsrsLastReviewUtc { get; set; }
        public DateTime? FsrsDueUtc { get; set; } = DateTime.UtcNow; // enqueue existing cards
    }
}
