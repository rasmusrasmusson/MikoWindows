using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage; // for ApplicationData (fallback scan)

namespace MikoMe.Data
{
    public static class LocalDbPatch
    {
        /// <summary>
        /// Patch the schema on an already-openable SQLite connection (Cards & Words).
        /// Adds id/updated_at/deleted_at, then back-fills updated_at.
        /// Safe to call every launch.
        /// </summary>
        public static async Task EnsureSyncSchemaAsync(DbConnection connection)
        {
            // Use caller's connection if it's already open; otherwise open it.
            var shouldClose = connection.State != System.Data.ConnectionState.Open;
            if (shouldClose) await connection.OpenAsync();

            await EnsureColumnsAsync(connection, "Words");

            // Append-only review log
            await ExecAsync(connection, @"
CREATE TABLE IF NOT EXISTS review_events (
  event_id      TEXT PRIMARY KEY,
  card_id       TEXT NOT NULL,
  user_id       TEXT NOT NULL,
  reviewed_at   TEXT NOT NULL,
  grade         INTEGER NOT NULL,
  interval_days INTEGER NOT NULL,
  stability     REAL,
  difficulty    REAL,
  payload       TEXT NOT NULL DEFAULT '{}'
);");

            // Outbox for unsent writes
            await ExecAsync(connection, @"
CREATE TABLE IF NOT EXISTS outbox (
  id          TEXT PRIMARY KEY,
  kind        TEXT NOT NULL,
  payload     TEXT NOT NULL,
  created_utc TEXT NOT NULL,
  attempts    INTEGER NOT NULL DEFAULT 0
);");

            if (shouldClose) await connection.CloseAsync();
        }

        /// <summary>
        /// Open by path and patch.
        /// </summary>
        public static async Task EnsureSyncSchemaAtPathAsync(string dbPath)
        {
            using var conn = new SqliteConnection($"Data Source={dbPath};Mode=ReadWrite");
            await EnsureSyncSchemaAsync(conn);
            System.Diagnostics.Debug.WriteLine($"[LocalDbPatch] Patched {dbPath}");
        }

        /// <summary>
        /// Fallback: scan LocalState & LocalCache recursively for SQLite files and patch any found.
        /// </summary>
        public static async Task<int> PatchAllDbsInLocalStateAsync()
        {
            int patched = 0;
            var roots = new[]
            {
                ApplicationData.Current.LocalFolder.Path,
                ApplicationData.Current.LocalCacheFolder.Path
            };

            foreach (var root in roots)
            {
                if (!Directory.Exists(root)) continue;
                foreach (var path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
                {
                    if (!LooksLikeSqlite(path)) continue;
                    try { await EnsureSyncSchemaAtPathAsync(path); patched++; }
                    catch (Exception ex)
                    { System.Diagnostics.Debug.WriteLine($"[LocalDbPatch] Skip {path}: {ex.Message}"); }
                }
            }
            return patched;
        }

        // ===== helpers =====

        private static async Task EnsureColumnsAsync(DbConnection conn, string table)
        {
            if (!await TableExistsAsync(conn, table)) return;

            var cols = await GetColumnsAsync(conn, table);

            // Add columns WITHOUT NOT NULL/DEFAULT (SQLite restriction on ALTER TABLE).
            if (!cols.Contains("id"))
                await ExecAsync(conn, $"ALTER TABLE {table} ADD COLUMN id TEXT");

            var addedUpdatedAt = false;
            if (!cols.Contains("updated_at"))
            {
                await ExecAsync(conn, $"ALTER TABLE {table} ADD COLUMN updated_at TEXT");
                addedUpdatedAt = true;
            }

            if (!cols.Contains("deleted_at"))
                await ExecAsync(conn, $"ALTER TABLE {table} ADD COLUMN deleted_at TEXT");

            // Back-fill updated_at if we just added it (or if it was null)
            // Use ISO8601 UTC (Z) so EF DateTimeOffset parses cleanly.
            await ExecAsync(conn, $"UPDATE {table} SET updated_at = COALESCE(updated_at, strftime('%Y-%m-%dT%H:%M:%fZ','now'))");
        }

        private static async Task<bool> TableExistsAsync(DbConnection conn, string name)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=$n";
            var p = cmd.CreateParameter(); p.ParameterName = "$n"; p.Value = name;
            cmd.Parameters.Add(p);
            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }

        private static async Task<HashSet<string>> GetColumnsAsync(DbConnection conn, string table)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA table_info({table})";
            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync()) set.Add(r.GetString(1)); // column name
            return set;
        }

        private static async Task ExecAsync(DbConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
            System.Diagnostics.Debug.WriteLine("[LocalDbPatch] " + sql);
        }

        private static bool LooksLikeSqlite(string path)
        {
            try
            {
                using var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (fs.Length < 16) return false;
                Span<byte> buf = stackalloc byte[16];
                fs.Read(buf);
                var header = System.Text.Encoding.ASCII.GetString(buf);
                return header.StartsWith("SQLite format 3");
            }
            catch { return false; }
        }
    }
}
