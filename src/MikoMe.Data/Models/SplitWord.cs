namespace MikoMe.Models
{
    public class SplitWord
    {
        public string Hanzi { get; set; }
        public string Pinyin { get; set; }
        public string English { get; set; }
        public bool IsSelected { get; set; } = true; // checked by default
    }
}
