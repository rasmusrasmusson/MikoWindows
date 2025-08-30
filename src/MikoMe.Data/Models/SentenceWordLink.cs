using System;
using Microsoft.EntityFrameworkCore;

namespace MikoMe.Models
{
    // ✅ This makes the composite PK explicit even if OnModelCreating is skipped
    [PrimaryKey(nameof(SentenceId), nameof(WordId))]
    public class SentenceWordLink
    {
        public int SentenceId { get; set; }     // FK -> Words.Id (sentence)
        public Word Sentence { get; set; } = null!;

        public int WordId { get; set; }     // FK -> Words.Id (token)
        public Word Word { get; set; } = null!;

        public int? Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
