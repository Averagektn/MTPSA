using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bank.Credits.Data.Migrations
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
                name: "CreditProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RepaymentSchedule = table.Column<int>(type: "INTEGER", nullable: false),
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
                    table.PrimaryKey("PK_CreditProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditProducts_ChartAccounts_InterestChartAccountId",
                        column: x => x.InterestChartAccountId,
                        principalTable: "ChartAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditProducts_ChartAccounts_PrincipalChartAccountId",
                        column: x => x.PrincipalChartAccountId,
                        principalTable: "ChartAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditProducts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreditContracts",
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
                    RemainingPrincipal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    AnnualRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    PrincipalAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterestAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    PaidPeriods = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPaymentOn = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditContracts_BankAccounts_InterestAccountId",
                        column: x => x.InterestAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditContracts_BankAccounts_PrincipalAccountId",
                        column: x => x.PrincipalAccountId,
                        principalTable: "BankAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditContracts_CreditProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CreditProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreditContracts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
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
                        name: "FK_LedgerEntries_CreditContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "CreditContracts",
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
                    { 3, "2410", "Short-term loans to individuals", "Краткосрочные кредиты физическим лицам", 1 },
                    { 4, "2420", "Long-term loans to individuals", "Долгосрочные кредиты физическим лицам", 1 },
                    { 5, "2471", "Accrued interest on short-term loans to individuals", "Начисленные проценты по краткосрочным кредитам физическим лицам", 1 },
                    { 6, "2472", "Accrued interest on long-term loans to individuals", "Начисленные проценты по долгосрочным кредитам физическим лицам", 1 },
                    { 7, "7327", "Bank development fund", "Фонд развития банка", 2 }
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
                    { 3, 7, null, 1, "Bank development fund", "Фонд развития банка", "7327000000010" }
                });

            migrationBuilder.InsertData(
                table: "CreditProducts",
                columns: new[] { "Id", "AnnualRate", "Code", "CurrencyId", "InterestChartAccountId", "MaxAmount", "MinAmount", "NameEn", "NameRu", "PrincipalChartAccountId", "RepaymentSchedule", "TermMonths" },
                values: new object[,]
                {
                    { 1, 18.1m, "ALFA_CASH", 1, 5, 20000m, 1000m, "Cash loan (annuity)", "Кредит наличными (аннуитет)", 3, 1, 12 },
                    { 2, 18.1m, "ALFA_ONLINE", 1, 6, 7000m, 500m, "Online loan (interest monthly, principal at term end)", "Кредит онлайн (проценты ежемесячно, тело в конце срока)", 4, 2, 24 }
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
                name: "IX_CreditContracts_CurrencyId",
                table: "CreditContracts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditContracts_InterestAccountId",
                table: "CreditContracts",
                column: "InterestAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditContracts_Number",
                table: "CreditContracts",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditContracts_PrincipalAccountId",
                table: "CreditContracts",
                column: "PrincipalAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditContracts_ProductId",
                table: "CreditContracts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditProducts_Code",
                table: "CreditProducts",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditProducts_CurrencyId",
                table: "CreditProducts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditProducts_InterestChartAccountId",
                table: "CreditProducts",
                column: "InterestChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditProducts_PrincipalChartAccountId",
                table: "CreditProducts",
                column: "PrincipalChartAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

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
                name: "CreditContracts");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "CreditProducts");

            migrationBuilder.DropTable(
                name: "ChartAccounts");

            migrationBuilder.DropTable(
                name: "Currencies");
        }
    }
}
