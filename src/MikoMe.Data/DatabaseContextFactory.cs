using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace MikoMe.Data
{
    // Ensures EF Tools use THIS context at design time (migrations/updates).
    public sealed class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>();

            // Use the SAME provider/connection you use at runtime.
            // If you already point to a file DB under LocalAppData, mirror it here:
            var dbDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MikoMe");
            Directory.CreateDirectory(dbDir);
            var dbPath = Path.Combine(dbDir, "miko.db");

            options.UseSqlite($"Data Source={dbPath}");

            return new DatabaseContext(options.Options);
        }
    }
}
