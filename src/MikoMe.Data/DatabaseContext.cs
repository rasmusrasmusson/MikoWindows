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

        // ✅ new join table
        public DbSet<SentenceWordLink> SentenceWordLinks => Set<SentenceWordLink>();

        private readonly string _dbPath;

        public DatabaseContext()
        {
            var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var dir = Path.Combine(docs, "MikoMe");
            Directory.CreateDirectory(dir);
            _dbPath = Path.Combine(dir, "MikoMe.db");

            // OK to keep for now; see “Migrations vs EnsureCreated” below
            Database.EnsureCreated();

            if (!Words.Any())
            {
                // your seed… (unchanged)
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlite($"Data Source={_dbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // your existing config (Word->Cards etc.) … unchanged

            // ✅ many-to-many between Words (sentence) and Words (token)
            modelBuilder.Entity<SentenceWordLink>(b =>
            {
                b.HasKey(x => new { x.SentenceId, x.WordId });

                b.HasOne(x => x.Sentence)
                 .WithMany(w => w.AsSentenceLinks)
                 .HasForeignKey(x => x.SentenceId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(x => x.Word)
                 .WithMany(w => w.AsWordLinks)
                 .HasForeignKey(x => x.WordId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(x => new { x.SentenceId, x.Order });
            });
        }
    }
}
