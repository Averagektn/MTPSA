using Bank.Credits.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank.Credits.Data.Migrations;

[DbContext(typeof(CreditsDbContext))]
[Migration("20260922230000_LedgerExchangeAmounts")]
public class LedgerExchangeAmounts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<decimal>(
            name: "AmountByn",
            table: "LedgerEntries",
            type: "TEXT",
            precision: 18,
            scale: 2,
            nullable: false,
            defaultValue: 0m);

        migrationBuilder.AddColumn<decimal>(
            name: "Rate",
            table: "LedgerEntries",
            type: "TEXT",
            precision: 18,
            scale: 6,
            nullable: false,
            defaultValue: 1m);

        migrationBuilder.Sql("""UPDATE "LedgerEntries" SET "AmountByn" = "Amount", "Rate" = 1;""");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "AmountByn", table: "LedgerEntries");
        migrationBuilder.DropColumn(name: "Rate", table: "LedgerEntries");
    }
}
