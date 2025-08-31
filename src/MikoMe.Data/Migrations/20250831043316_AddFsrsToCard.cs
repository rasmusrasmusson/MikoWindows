using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikoMe.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFsrsToCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    Grade = table.Column<int>(type: "INTEGER", nullable: false),
                    PrevInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    NextInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    PrevEase = table.Column<double>(type: "REAL", nullable: false),
                    NextEase = table.Column<double>(type: "REAL", nullable: false),
                    ElapsedDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Hanzi = table.Column<string>(type: "TEXT", nullable: false),
                    Pinyin = table.Column<string>(type: "TEXT", nullable: false),
                    English = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WordId = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<int>(type: "INTEGER", nullable: false),
                    DueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IntervalDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Ease = table.Column<double>(type: "REAL", nullable: false),
                    Reps = table.Column<int>(type: "INTEGER", nullable: false),
                    Lapses = table.Column<int>(type: "INTEGER", nullable: false),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    LastReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FsrsStability = table.Column<double>(type: "REAL", nullable: true),
                    FsrsDifficulty = table.Column<double>(type: "REAL", nullable: true),
                    FsrsIsNew = table.Column<bool>(type: "INTEGER", nullable: false),
                    FsrsReps = table.Column<int>(type: "INTEGER", nullable: false),
                    FsrsLapses = table.Column<int>(type: "INTEGER", nullable: false),
                    FsrsLastReviewUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FsrsDueUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SentenceWordLinks",
                columns: table => new
                {
                    SentenceId = table.Column<int>(type: "INTEGER", nullable: false),
                    WordId = table.Column<int>(type: "INTEGER", nullable: false),
                    Order = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentenceWordLinks", x => new { x.SentenceId, x.WordId });
                    table.ForeignKey(
                        name: "FK_SentenceWordLinks_Words_SentenceId",
                        column: x => x.SentenceId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SentenceWordLinks_Words_WordId",
                        column: x => x.WordId,
                        principalTable: "Words",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_FsrsDueUtc",
                table: "Cards",
                column: "FsrsDueUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Cards_WordId",
                table: "Cards",
                column: "WordId");

            migrationBuilder.CreateIndex(
                name: "IX_SentenceWordLinks_WordId",
                table: "SentenceWordLinks",
                column: "WordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "ReviewLogs");

            migrationBuilder.DropTable(
                name: "SentenceWordLinks");

            migrationBuilder.DropTable(
                name: "Words");
        }
    }
}
