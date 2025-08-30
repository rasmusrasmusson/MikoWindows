using Microsoft.EntityFrameworkCore;
using MikoMe.Models;
using System;
using System.IO;
using System.Linq;

namespace MikoMe.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Word> Words => Set<Word>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();

        private readonly string _dbPath;

        public DatabaseContext()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = Path.Combine(docs, "MikoMe");
            Directory.CreateDirectory(dir);
            _dbPath = Path.Combine(dir, "MikoMe.db");
            Database.EnsureCreated();

            if (!Words.Any())
            {
                var samples = new[]
                {
                    new Word { English = "hello", Hanzi = "你好", Pinyin = "nǐ hǎo" },
                    new Word { English = "thank you", Hanzi = "谢谢", Pinyin = "xiè xie" },
                    new Word { English = "goodbye", Hanzi = "再见", Pinyin = "zài jiàn" },
                    new Word { English = "to study", Hanzi = "学习", Pinyin = "xué xí" }
                };
                Words.AddRange(samples);
                SaveChanges();

                foreach (var w in Words)
                {
                    Cards.Add(new Card { WordId = w.Id, Direction = CardDirection.ZhToEn, DueAtUtc = DateTime.UtcNow });
                    Cards.Add(new Card { WordId = w.Id, Direction = CardDirection.EnToZh, DueAtUtc = DateTime.UtcNow });
                }
                SaveChanges();
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={_dbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Word>()
                .HasMany(w => w.Cards)
                .WithOne(c => c.Word)
                .HasForeignKey(c => c.WordId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Card>()
                .Property(c => c.Direction)
                .HasConversion<int>();
        }
    }
}
