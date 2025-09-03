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

        // ------- Legacy SM-2-ish fields (kept for back-compat; now nullable) -------
        [Obsolete("Legacy field. Use FsrsDueUtc instead.")]
        public DateTime? DueAtUtc { get; set; }

        [Obsolete("Legacy field. FSRS computes intervals internally.")]
        public int? IntervalDays { get; set; }

        [Obsolete("Legacy field. FSRS uses Difficulty (1..10) instead.")]
        public double? Ease { get; set; }

        [Obsolete("Legacy field. Use FsrsReps/FsrsLapses.")]
        public int? Reps { get; set; }

        [Obsolete("Legacy field. Use FsrsLapses.")]
        public int? Lapses { get; set; }

        [Obsolete("Legacy field. FSRS doesn't use SM-2 state strings.")]
        public string? State { get; set; }

        [Obsolete("Legacy field. Use FsrsLastReviewUtc.")]
        public DateTime? LastReviewedAtUtc { get; set; }

        // ----------------------------- FSRS state (nullable for old rows) -----------------------------
        public double? FsrsStability { get; set; }   // S (days)
        public double? FsrsDifficulty { get; set; }   // D [1..10]
        public bool? FsrsIsNew { get; set; }
        public int? FsrsReps { get; set; }
        public int? FsrsLapses { get; set; }
        public DateTime? FsrsLastReviewUtc { get; set; }
        public DateTime? FsrsDueUtc { get; set; }
    }
}
