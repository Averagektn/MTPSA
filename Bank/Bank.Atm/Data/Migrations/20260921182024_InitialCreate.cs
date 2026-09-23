using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.Atm.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SessionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrintedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    BodyJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Screen = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 16, nullable: true),
                    Pin = table.Column<string>(type: "TEXT", maxLength: 4, nullable: true),
                    PinAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    Authorized = table.Column<bool>(type: "INTEGER", nullable: false),
                    FieldsJson = table.Column<string>(type: "TEXT", nullable: false),
                    AccountJson = table.Column<string>(type: "TEXT", nullable: true),
                    LastResultJson = table.Column<string>(type: "TEXT", nullable: true),
                    ReceiptJson = table.Column<string>(type: "TEXT", nullable: true),
                    MessageKey = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_SessionId",
                table: "Receipts",
                column: "SessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
