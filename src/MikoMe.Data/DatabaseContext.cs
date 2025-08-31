using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using MikoMe.Models;

namespace MikoMe.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) { }

        // ---- DbSets ----
        public DbSet<Word> Words { get; set; } = default!;
        public DbSet<Card> Cards { get; set; } = default!;
        public DbSet<ReviewLog> ReviewLogs { get; set; } = default!;
        public DbSet<SentenceWordLink> SentenceWordLinks { get; set; } = default!;
        public DbSet<SplitWord> SplitWords { get; set; } = default!;
        public DbSet<TokenItem> TokenItems { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MikoMe");
                Directory.CreateDirectory(dir);
                var dbPath = Path.Combine(dir, "miko.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Card → Word (many cards per word)
            modelBuilder.Entity<Card>()
                .HasOne(c => c.Word)
                .WithMany(w => w.Cards)
                .HasForeignKey(c => c.WordId)
                .OnDelete(DeleteBehavior.Cascade);

            // Keyless projections (no PK; not mapped to a table in migrations)
            modelBuilder.Entity<SplitWord>().HasNoKey().ToView((string?)null);
            modelBuilder.Entity<TokenItem>().HasNoKey().ToView((string?)null);

            // If SentenceWordLink uses a composite key, uncomment and adjust:
            // modelBuilder.Entity<SentenceWordLink>()
            //     .HasKey(l => new { l.SentenceId, l.WordId, l.Order });

            base.OnModelCreating(modelBuilder);
        }
    }
}
