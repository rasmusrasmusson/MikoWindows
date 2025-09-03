using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MikoMe.Models;

namespace MikoMe.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // DbSets (from your snapshot)
        public DbSet<Word> Words => Set<Word>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<ReviewLog> ReviewLogs => Set<ReviewLog>();
        public DbSet<SentenceWordLink> SentenceWordLinks => Set<SentenceWordLink>();

        // Optional query types (keyless)
        public DbSet<SplitWord> SplitWords => Set<SplitWord>();
        public DbSet<TokenItem> TokenItems => Set<TokenItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---- Mirror key/index config visible in the migration snapshot ----
            // Cards
            modelBuilder.Entity<Card>(b =>
            {
                b.HasIndex(c => c.FsrsDueUtc);
                b.HasIndex(c => c.WordId);
                b.HasOne(c => c.Word)
                 .WithMany(w => w.Cards)
                 .HasForeignKey(c => c.WordId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // SentenceWordLink (composite key)
            modelBuilder.Entity<SentenceWordLink>(b =>
            {
                b.HasKey(x => new { x.SentenceId, x.WordId });
                b.HasOne<Word>(x => x.Sentence)
                 .WithMany(w => w.AsSentenceLinks)
                 .HasForeignKey(x => x.SentenceId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasOne<Word>(x => x.Word)
                 .WithMany(w => w.AsWordLinks)
                 .HasForeignKey(x => x.WordId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Keyless projections (match snapshot's ToTable(null)/ToView(null))
            modelBuilder.Entity<SplitWord>().HasNoKey().ToView((string?)null);
            modelBuilder.Entity<TokenItem>().HasNoKey().ToView((string?)null);

            // ---- Add sync shadow properties on the physical tables we sync ----
            AddShadowSyncProperties(modelBuilder); // id / updated_at / deleted_at on Cards & Words
        }

        // ========== SaveChanges stamping ==========
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            StampSyncColumns();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            StampSyncColumns();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        // ======== helpers ========

        // IMPORTANT: these must match your real table names (case-insensitive).
        // From your snapshot they are "Cards" and "Words".
        private static readonly string[] __SyncTables = new[] { "Cards", "Words" };

        private void AddShadowSyncProperties(ModelBuilder modelBuilder)
        {
            foreach (var et in modelBuilder.Model.GetEntityTypes())
            {
                var table = et.GetTableName();
                if (table is null) continue;
                if (!__SyncTables.Contains(table, StringComparer.OrdinalIgnoreCase)) continue;

                var entity = modelBuilder.Entity(et.ClrType);

                // Add shadow columns only if not already mapped
                if (et.FindProperty("id") == null)
                    entity.Property<string>("id");

                if (et.FindProperty("updated_at") == null)
                    entity.Property<DateTimeOffset>("updated_at");

                if (et.FindProperty("deleted_at") == null)
                    entity.Property<DateTimeOffset?>("deleted_at");
            }
        }

        private void StampSyncColumns()
        {
            var now = DateTimeOffset.UtcNow;

            var tracked = ChangeTracker.Entries()
                .Where(e =>
                    (e.State == EntityState.Added || e.State == EntityState.Modified) &&
                    __SyncTables.Contains(e.Metadata.GetTableName() ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var e in tracked)
            {
                // Ensure GUID string id on insert (shadow property)
                var idProp = e.Properties.FirstOrDefault(p => p.Metadata.Name == "id");
                if (e.State == EntityState.Added && idProp is not null)
                {
                    var hasValue = idProp.CurrentValue is string s && !string.IsNullOrWhiteSpace(s);
                    if (!hasValue) idProp.CurrentValue = Guid.NewGuid().ToString();
                }

                // Always bump updated_at
                var upd = e.Properties.FirstOrDefault(p => p.Metadata.Name == "updated_at");
                if (upd is not null) upd.CurrentValue = now;
            }
        }
    }
}
