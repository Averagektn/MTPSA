using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bank.Credits.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtmCards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CardNumber = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    Pin = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    CreditContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    CardAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankCards_BankAccounts_CardAccountId",
                        column: x => x.CardAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankCards_CreditContracts_CreditContractId",
                        column: x => x.CreditContractId,
                        principalTable: "CreditContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "ChartAccounts",
                columns: new[] { "Id", "Code", "NameEn", "NameRu", "Nature" },
                values: new object[,]
                {
                    { 8, "3014", "Card accounts of individuals", "Карт-счета физических лиц", 2 },
                    { 9, "3819", "Settlements with mobile operators", "Расчёты с операторами связи", 2 }
                });

            migrationBuilder.InsertData(
                table: "BankAccounts",
                columns: new[] { "Id", "ChartAccountId", "ClientId", "CurrencyId", "NameEn", "NameRu", "Number" },
                values: new object[] { 4, 9, null, 1, "Settlements with mobile operators", "Расчёты с операторами связи", "3819000000018" });

            migrationBuilder.CreateIndex(
                name: "IX_BankCards_CardAccountId",
                table: "BankCards",
                column: "CardAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankCards_CardNumber",
                table: "BankCards",
                column: "CardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankCards_CreditContractId",
                table: "BankCards",
                column: "CreditContractId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankCards");

            migrationBuilder.DeleteData(
                table: "BankAccounts",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "ChartAccounts",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "ChartAccounts",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
