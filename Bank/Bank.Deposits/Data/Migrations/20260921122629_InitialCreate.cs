using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bank.Deposits.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BankStates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CurrentDate = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChartAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Nature = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    ChartAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: true),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_ChartAccounts_ChartAccountId",
                        column: x => x.ChartAccountId,
                        principalTable: "ChartAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepositProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Revocable = table.Column<bool>(type: "INTEGER", nullable: false),
                    InterestSchedule = table.Column<int>(type: "INTEGER", nullable: false),
                    TermMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    AnnualRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MinAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    MaxAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrincipalChartAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestChartAccountId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositProducts_ChartAccounts_InterestChartAccountId",
                        column: x => x.InterestChartAccountId,
                        principalTable: "ChartAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepositProducts_ChartAccounts_PrincipalChartAccountId",
                        column: x => x.PrincipalChartAccountId,
                        principalTable: "ChartAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepositProducts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DepositContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Number = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrencyId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    TermMonths = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AnnualRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PrincipalAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    LastInterestPaidOn = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepositContracts_BankAccounts_InterestAccountId",
                        column: x => x.InterestAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepositContracts_BankAccounts_PrincipalAccountId",
                        column: x => x.PrincipalAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepositContracts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DepositContracts_DepositProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "DepositProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LedgerEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DebitAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreditAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Operation = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ContractId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_BankAccounts_CreditAccountId",
                        column: x => x.CreditAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_BankAccounts_DebitAccountId",
                        column: x => x.DebitAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LedgerEntries_DepositContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "DepositContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BankStates",
                columns: new[] { "Id", "CurrentDate" },
                values: new object[] { 1, new DateOnly(2026, 9, 20) });

            migrationBuilder.InsertData(
                table: "ChartAccounts",
                columns: new[] { "Id", "Code", "NameEn", "NameRu", "Nature" },
                values: new object[,]
                {
                    { 1, "1010", "Bank cash", "Касса банка", 1 },
                    { 2, "1201", "Correspondent account in the National Bank of Belarus", "Корреспондентский счёт в НБ РБ", 1 },
                    { 3, "3014", "Current accounts of individuals", "Текущие счета физических лиц", 2 },
                    { 4, "3404", "Demand deposits of individuals", "Вклады до востребования физических лиц", 2 },
                    { 5, "3414", "Term deposits of individuals", "Срочные вклады физических лиц", 2 },
                    { 6, "3470", "Accrued interest on demand deposits", "Начисленные проценты по вкладам до востребования", 2 },
                    { 7, "3471", "Accrued interest on term deposits", "Начисленные проценты по срочным вкладам", 2 },
                    { 8, "7327", "Bank development fund", "Фонд развития банка", 2 }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { 1, "BYN", "Belarusian ruble", "Белорусский рубль" },
                    { 2, "USD", "US dollar", "Доллар США" },
                    { 3, "EUR", "Euro", "Евро" }
                });

            migrationBuilder.InsertData(
                table: "BankAccounts",
                columns: new[] { "Id", "ChartAccountId", "ClientId", "CurrencyId", "NameEn", "NameRu", "Number" },
                values: new object[,]
                {
                    { 1, 1, null, 1, "Bank cash", "Касса банка", "1010000000013" },
                    { 2, 2, null, 1, "Correspondent account in the National Bank of Belarus", "Корреспондентский счёт в НБ РБ", "1201000000019" },
                    { 3, 8, null, 1, "Bank development fund", "Фонд развития банка", "7327000000010" }
                });

            migrationBuilder.InsertData(
                table: "DepositProducts",
                columns: new[] { "Id", "AnnualRate", "Code", "CurrencyId", "InterestChartAccountId", "InterestSchedule", "MaxAmount", "MinAmount", "NameEn", "NameRu", "PrincipalChartAccountId", "Revocable", "TermMonths" },
                values: new object[,]
                {
                    { 1, 5.5m, "ALFA_SAFE", 1, 6, 1, 20000m, 200m, "Alfa Safe (revocable)", "Альфа Сейф (отзывный)", 4, true, 13 },
                    { 2, 12m, "ALFA_VKLAD", 1, 7, 2, 5000000m, 50m, "Alfa Vklad (irrevocable)", "Альфа Вклад (безотзывный)", 5, false, 13 }
                });

            migrationBuilder.InsertData(
                table: "LedgerEntries",
                columns: new[] { "Id", "Amount", "BookedOn", "ContractId", "CreditAccountId", "DebitAccountId", "Operation" },
                values: new object[] { 1, 1000000m, new DateOnly(2026, 9, 20), null, 3, 2, "sfrbCapital" });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_ChartAccountId",
                table: "BankAccounts",
                column: "ChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CurrencyId",
                table: "BankAccounts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_Number",
                table: "BankAccounts",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartAccounts_Code",
                table: "ChartAccounts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositContracts_CurrencyId",
                table: "DepositContracts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositContracts_InterestAccountId",
                table: "DepositContracts",
                column: "InterestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositContracts_Number",
                table: "DepositContracts",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositContracts_PrincipalAccountId",
                table: "DepositContracts",
                column: "PrincipalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositContracts_ProductId",
                table: "DepositContracts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositProducts_Code",
                table: "DepositProducts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepositProducts_CurrencyId",
                table: "DepositProducts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositProducts_InterestChartAccountId",
                table: "DepositProducts",
                column: "InterestChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositProducts_PrincipalChartAccountId",
                table: "DepositProducts",
                column: "PrincipalChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_ContractId",
                table: "LedgerEntries",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_CreditAccountId",
                table: "LedgerEntries",
                column: "CreditAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LedgerEntries_DebitAccountId",
                table: "LedgerEntries",
                column: "DebitAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BankStates");

            migrationBuilder.DropTable(
                name: "LedgerEntries");

            migrationBuilder.DropTable(
                name: "DepositContracts");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "DepositProducts");

            migrationBuilder.DropTable(
                name: "ChartAccounts");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
