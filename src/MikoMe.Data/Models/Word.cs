using System.ComponentModel.DataAnnotations.Schema; // needed for InverseProperty
using System.Collections.Generic;
using System;


namespace MikoMe.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string Hanzi { get; set; } = string.Empty;
        public string Pinyin { get; set; } = string.Empty;
        public string English { get; set; } = string.Empty;

        public ICollection<Card> Cards { get; set; } = new List<Card>();

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        [InverseProperty(nameof(SentenceWordLink.Sentence))]
        public ICollection<SentenceWordLink> AsSentenceLinks { get; set; } = new List<SentenceWordLink>();

        [InverseProperty(nameof(SentenceWordLink.Word))]
        public ICollection<SentenceWordLink> AsWordLinks { get; set; } = new List<SentenceWordLink>();
    }
}
