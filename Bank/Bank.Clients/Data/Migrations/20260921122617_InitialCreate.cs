using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bank.Clients.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citizenships",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citizenships", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Disabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disabilities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaritalStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NameEn = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NameRu = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaritalStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Patronymic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PassportSeries = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    PassportNumber = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    IssuedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IssueDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    IdentificationNumber = table.Column<string>(type: "TEXT", maxLength: 14, nullable: false),
                    BirthPlace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    ResidenceCityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ResidenceAddress = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    RegistrationCityId = table.Column<int>(type: "INTEGER", nullable: false),
                    HomePhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    MobilePhone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Workplace = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    MaritalStatusId = table.Column<int>(type: "INTEGER", nullable: false),
                    CitizenshipId = table.Column<int>(type: "INTEGER", nullable: false),
                    DisabilityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Pensioner = table.Column<bool>(type: "INTEGER", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Cities_RegistrationCityId",
                        column: x => x.RegistrationCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Cities_ResidenceCityId",
                        column: x => x.ResidenceCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Citizenships_CitizenshipId",
                        column: x => x.CitizenshipId,
                        principalTable: "Citizenships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_Disabilities_DisabilityId",
                        column: x => x.DisabilityId,
                        principalTable: "Disabilities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Clients_MaritalStatuses_MaritalStatusId",
                        column: x => x.MaritalStatusId,
                        principalTable: "MaritalStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { 1, "Minsk", "Минск" },
                    { 2, "Brest", "Брест" },
                    { 3, "Grodno", "Гродно" },
                    { 4, "Vitebsk", "Витебск" },
                    { 5, "Mogilev", "Могилёв" },
                    { 6, "Gomel", "Гомель" }
                });

            migrationBuilder.InsertData(
                table: "Citizenships",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { 1, "Belarus", "Беларусь" },
                    { 2, "Russia", "Россия" },
                    { 3, "Kazakhstan", "Казахстан" },
                    { 4, "Other", "Другое" }
                });

            migrationBuilder.InsertData(
                table: "Disabilities",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { 1, "None", "Нет" },
                    { 2, "Group I", "I группа" },
                    { 3, "Group II", "II группа" },
                    { 4, "Group III", "III группа" }
                });

            migrationBuilder.InsertData(
                table: "MaritalStatuses",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { 1, "Single", "Холост / не замужем" },
                    { 2, "Married", "Женат / замужем" },
                    { 3, "Divorced", "Разведён(а)" },
                    { 4, "Widowed", "Вдовец / вдова" }
                });

            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "Id", "BirthDate", "BirthPlace", "CitizenshipId", "DisabilityId", "Email", "FirstName", "HomePhone", "IdentificationNumber", "IssueDate", "IssuedBy", "LastName", "MaritalStatusId", "MobilePhone", "MonthlyIncome", "PassportNumber", "PassportSeries", "Patronymic", "Pensioner", "Position", "RegistrationCityId", "ResidenceAddress", "ResidenceCityId", "Workplace" },
                values: new object[,]
                {
                    { 1, new DateOnly(2003, 5, 15), "Minsk", 1, 1, "n.glushachenko@example.com", "Nikita", "+375 (17) 234-56-78", "1505033A015PB7", new DateOnly(2019, 6, 20), "Central District Police Department, Minsk", "Glushachenko", 1, "+375 (29) 111-22-33", 900m, "7654321", "AB", "Sergeevich", false, "Student", 1, "Independence Ave. 10, apt. 5", 1, "BSUIR" },
                    { 2, new DateOnly(1988, 3, 12), "Minsk", 1, 1, "p.ivanov@example.com", "Petr", null, "1203881A013PB2", new DateOnly(2015, 4, 1), "Moskovsky District Police Department, Minsk", "Ivanov", 2, "+375 (29) 555-44-33", 2400m, "1234567", "BM", "Alekseevich", false, "Economist", 1, "Pobediteley Ave. 21, apt. 14", 1, "Belarusbank" },
                    { 3, new DateOnly(1991, 11, 2), "Brest", 1, 1, null, "Maria", "+375 (16) 221-10-10", "0211912B024PB3", new DateOnly(2016, 11, 15), "Leninsky District Police Department, Brest", "Petrova", 2, "+375 (33) 222-33-44", 1800m, "2345678", "HB", "Alexandrovna", false, "Accountant", 2, "Sovetskaya St. 8, apt. 3", 2, "Horizon Ltd." },
                    { 4, new DateOnly(1975, 7, 28), "Grodno", 1, 4, "a.sidorov@example.com", "Alexey", null, "2807753C035PB4", new DateOnly(2014, 8, 10), "Oktyabrsky District Police Department, Grodno", "Sidorov", 3, "+375 (44) 777-88-99", 2100m, "3456789", "KH", "Petrovich", false, "Engineer", 3, "Gorky St. 45, apt. 12", 3, "Grodno Azot" },
                    { 5, new DateOnly(1958, 1, 9), "Vitebsk", 1, 3, null, "Anna", "+375 (21) 360-12-34", "0901584K046PB5", new DateOnly(2018, 2, 14), "Zheleznodorozhny District Police Department, Vitebsk", "Kozlova", 4, null, 650m, "4567890", "MP", "Viktorovna", true, null, 4, "Lenin St. 3, apt. 7", 4, null },
                    { 6, new DateOnly(1995, 9, 21), "Gomel", 2, 1, "d.novikov@example.com", "Dmitry", null, "2109955E057PB6", new DateOnly(2021, 10, 5), "Central District Police Department, Gomel", "Novikov", 1, "+375 (25) 101-20-30", 3500m, "5678901", "MC", "Olegovich", false, "Software Developer", 1, "Barykina St. 120, apt. 44", 6, "EPAM Systems" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CitizenshipId",
                table: "Clients",
                column: "CitizenshipId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_DisabilityId",
                table: "Clients",
                column: "DisabilityId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_IdentificationNumber",
                table: "Clients",
                column: "IdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LastName_FirstName_Patronymic_BirthDate",
                table: "Clients",
                columns: new[] { "LastName", "FirstName", "Patronymic", "BirthDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_MaritalStatusId",
                table: "Clients",
                column: "MaritalStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_PassportSeries_PassportNumber",
                table: "Clients",
                columns: new[] { "PassportSeries", "PassportNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_RegistrationCityId",
                table: "Clients",
                column: "RegistrationCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ResidenceCityId",
                table: "Clients",
                column: "ResidenceCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Citizenships");

            migrationBuilder.DropTable(
                name: "Disabilities");

            migrationBuilder.DropTable(
                name: "MaritalStatuses");
        }
    }
}
