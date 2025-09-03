using System;
using System.Data.Common;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MikoMe.Models;

namespace MikoMe.Data
{
    public static class OutboxWriter
    {
        // Serialize with camelCase to match the server JSON we’ll build later
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Enqueue a cloud "PutCard" for the given Word. Uses the Word’s shadow sync id.
        /// Call this right AFTER db.SaveChangesAsync() for the Word.
        /// </summary>
        public static async Task EnqueuePutWordAsync(DatabaseContext db, Word w)
        {
            // Read the shadow properties we added (sync id + updated_at)
            var entry = db.Entry(w);
            var syncId = entry.Property<string>("id").CurrentValue;
            if (string.IsNullOrWhiteSpace(syncId))
            {
                // Should already be stamped by SaveChanges override, but be defensive
                syncId = Guid.NewGuid().ToString();
                entry.Property<string>("id").CurrentValue = syncId;
            }
            var updatedAt = entry.Property<DateTimeOffset>("updated_at").CurrentValue;

            // Build a minimal payload the server will accept as a "card"
            var payload = new
            {
                card_id = syncId,            // server key
                // deck_id can be added later when you introduce decks
                fields = new { hanzi = w.Hanzi, pinyin = w.Pinyin, english = w.English },
                meta = new { wordId = w.Id }, // local int id (diagnostic)
                updated_at = updatedAt,       // stamped by SaveChanges
                deleted_at = (DateTimeOffset?)null
            };

            var json = JsonSerializer.Serialize(payload, _json);
            await InsertOutboxAsync(db.Database.GetDbConnection(), Guid.NewGuid().ToString(), "PutCard", json);
        }

        /// <summary>Enqueue a review event (call when user reviews a card).</summary>
        public static async Task EnqueueReviewAsync(DatabaseContext db, string cardSyncId, Guid userId,
            DateTimeOffset reviewedAtUtc, short grade, int intervalDays, double? stability, double? difficulty)
        {
            var payload = new
            {
                event_id = Guid.NewGuid(),    // idempotency key; the server will also accept it
                card_id = cardSyncId,
                user_id = userId,
                reviewed_at = reviewedAtUtc,
                grade,
                interval_days = intervalDays,
                stability,
                difficulty,
                payload = new { device = "winui", app = "MikoMe" }
            };

            var json = JsonSerializer.Serialize(payload, _json);
            await InsertOutboxAsync(db.Database.GetDbConnection(), Guid.NewGuid().ToString(), "Review", json);
        }

        private static async Task InsertOutboxAsync(DbConnection conn, string id, string kind, string payload)
        {
            var shouldClose = conn.State != System.Data.ConnectionState.Open;
            if (shouldClose) await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT OR REPLACE INTO outbox (id, kind, payload, created_utc, attempts)
VALUES ($id, $kind, $payload, $created, 0)";
            cmd.Parameters.Add(new SqliteParameter("$id", id));
            cmd.Parameters.Add(new SqliteParameter("$kind", kind));
            cmd.Parameters.Add(new SqliteParameter("$payload", payload));
            cmd.Parameters.Add(new SqliteParameter("$created", DateTimeOffset.UtcNow.ToString("O")));

            await cmd.ExecuteNonQueryAsync();

            if (shouldClose) await conn.CloseAsync();
        }
    }
}
