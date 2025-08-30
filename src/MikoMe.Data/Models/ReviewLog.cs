namespace MikoMe.Models
{
    public class ReviewLog
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public DateTime ReviewedAtUtc { get; set; }
        public bool Success { get; set; }

        // Add missing props
        public int Grade { get; set; }
        public int PrevInterval { get; set; }
        public int NextInterval { get; set; }
        public double PrevEase { get; set; }
        public double NextEase { get; set; }
        public int ElapsedDays { get; set; }
    }
}
