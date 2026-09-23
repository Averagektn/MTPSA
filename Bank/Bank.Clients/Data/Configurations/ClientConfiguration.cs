using Bank.Clients.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bank.Clients.Data.Configurations;

public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Patronymic).HasMaxLength(100).IsRequired();
        builder.Property(c => c.PassportSeries).HasMaxLength(2).IsRequired();
        builder.Property(c => c.PassportNumber).HasMaxLength(7).IsRequired();
        builder.Property(c => c.IssuedBy).HasMaxLength(200).IsRequired();
        builder.Property(c => c.IdentificationNumber).HasMaxLength(14).IsRequired();
        builder.Property(c => c.BirthPlace).HasMaxLength(200).IsRequired();
        builder.Property(c => c.ResidenceAddress).HasMaxLength(300).IsRequired();
        builder.Property(c => c.HomePhone).HasMaxLength(30);
        builder.Property(c => c.MobilePhone).HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Workplace).HasMaxLength(200);
        builder.Property(c => c.Position).HasMaxLength(200);
        builder.Property(c => c.MonthlyIncome).HasPrecision(18, 2);

        builder.HasIndex(c => new { c.PassportSeries, c.PassportNumber }).IsUnique();
        builder.HasIndex(c => c.IdentificationNumber).IsUnique();
        builder.HasIndex(c => new { c.LastName, c.FirstName, c.Patronymic, c.BirthDate }).IsUnique();

        builder.HasOne(c => c.ResidenceCity)
            .WithMany()
            .HasForeignKey(c => c.ResidenceCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.RegistrationCity)
            .WithMany()
            .HasForeignKey(c => c.RegistrationCityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.MaritalStatus)
            .WithMany()
            .HasForeignKey(c => c.MaritalStatusId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Citizenship)
            .WithMany()
            .HasForeignKey(c => c.CitizenshipId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Disability)
            .WithMany()
            .HasForeignKey(c => c.DisabilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Client
            {
                Id = 1,
                LastName = "Glushachenko",
                FirstName = "Nikita",
                Patronymic = "Sergeevich",
                BirthDate = new DateOnly(2003, 5, 15),
                PassportSeries = "AB",
                PassportNumber = "7654321",
                IssuedBy = "Central District Police Department, Minsk",
                IssueDate = new DateOnly(2019, 6, 20),
                IdentificationNumber = "1505033A015PB7",
                BirthPlace = "Minsk",
                ResidenceCityId = 1,
                ResidenceAddress = "Independence Ave. 10, apt. 5",
                RegistrationCityId = 1,
                HomePhone = "+375 (17) 234-56-78",
                MobilePhone = "+375 (29) 111-22-33",
                Email = "n.glushachenko@example.com",
                Workplace = "BSUIR",
                Position = "Student",
                MaritalStatusId = 1,
                CitizenshipId = 1,
                DisabilityId = 1,
                Pensioner = false,
                MonthlyIncome = 900
            },
            new Client
            {
                Id = 2,
                LastName = "Ivanov",
                FirstName = "Petr",
                Patronymic = "Alekseevich",
                BirthDate = new DateOnly(1988, 3, 12),
                PassportSeries = "BM",
                PassportNumber = "1234567",
                IssuedBy = "Moskovsky District Police Department, Minsk",
                IssueDate = new DateOnly(2015, 4, 1),
                IdentificationNumber = "1203881A013PB2",
                BirthPlace = "Minsk",
                ResidenceCityId = 1,
                ResidenceAddress = "Pobediteley Ave. 21, apt. 14",
                RegistrationCityId = 1,
                MobilePhone = "+375 (29) 555-44-33",
                Email = "p.ivanov@example.com",
                Workplace = "Belarusbank",
                Position = "Economist",
                MaritalStatusId = 2,
                CitizenshipId = 1,
                DisabilityId = 1,
                Pensioner = false,
                MonthlyIncome = 2400
            },
            new Client
            {
                Id = 3,
                LastName = "Petrova",
                FirstName = "Maria",
                Patronymic = "Alexandrovna",
                BirthDate = new DateOnly(1991, 11, 2),
                PassportSeries = "HB",
                PassportNumber = "2345678",
                IssuedBy = "Leninsky District Police Department, Brest",
                IssueDate = new DateOnly(2016, 11, 15),
                IdentificationNumber = "0211912B024PB3",
                BirthPlace = "Brest",
                ResidenceCityId = 2,
                ResidenceAddress = "Sovetskaya St. 8, apt. 3",
                RegistrationCityId = 2,
                HomePhone = "+375 (16) 221-10-10",
                MobilePhone = "+375 (33) 222-33-44",
                Workplace = "Horizon Ltd.",
                Position = "Accountant",
                MaritalStatusId = 2,
                CitizenshipId = 1,
                DisabilityId = 1,
                Pensioner = false,
                MonthlyIncome = 1800
            },
            new Client
            {
                Id = 4,
                LastName = "Sidorov",
                FirstName = "Alexey",
                Patronymic = "Petrovich",
                BirthDate = new DateOnly(1975, 7, 28),
                PassportSeries = "KH",
                PassportNumber = "3456789",
                IssuedBy = "Oktyabrsky District Police Department, Grodno",
                IssueDate = new DateOnly(2014, 8, 10),
                IdentificationNumber = "2807753C035PB4",
                BirthPlace = "Grodno",
                ResidenceCityId = 3,
                ResidenceAddress = "Gorky St. 45, apt. 12",
                RegistrationCityId = 3,
                MobilePhone = "+375 (44) 777-88-99",
                Email = "a.sidorov@example.com",
                Workplace = "Grodno Azot",
                Position = "Engineer",
                MaritalStatusId = 3,
                CitizenshipId = 1,
                DisabilityId = 4,
                Pensioner = false,
                MonthlyIncome = 2100
            },
            new Client
            {
                Id = 5,
                LastName = "Kozlova",
                FirstName = "Anna",
                Patronymic = "Viktorovna",
                BirthDate = new DateOnly(1958, 1, 9),
                PassportSeries = "MP",
                PassportNumber = "4567890",
                IssuedBy = "Zheleznodorozhny District Police Department, Vitebsk",
                IssueDate = new DateOnly(2018, 2, 14),
                IdentificationNumber = "0901584K046PB5",
                BirthPlace = "Vitebsk",
                ResidenceCityId = 4,
                ResidenceAddress = "Lenin St. 3, apt. 7",
                RegistrationCityId = 4,
                HomePhone = "+375 (21) 360-12-34",
                MaritalStatusId = 4,
                CitizenshipId = 1,
                DisabilityId = 3,
                Pensioner = true,
                MonthlyIncome = 650
            },
            new Client
            {
                Id = 6,
                LastName = "Novikov",
                FirstName = "Dmitry",
                Patronymic = "Olegovich",
                BirthDate = new DateOnly(1995, 9, 21),
                PassportSeries = "MC",
                PassportNumber = "5678901",
                IssuedBy = "Central District Police Department, Gomel",
                IssueDate = new DateOnly(2021, 10, 5),
                IdentificationNumber = "2109955E057PB6",
                BirthPlace = "Gomel",
                ResidenceCityId = 6,
                ResidenceAddress = "Barykina St. 120, apt. 44",
                RegistrationCityId = 1,
                MobilePhone = "+375 (25) 101-20-30",
                Email = "d.novikov@example.com",
                Workplace = "EPAM Systems",
                Position = "Software Developer",
                MaritalStatusId = 1,
                CitizenshipId = 2,
                DisabilityId = 1,
                Pensioner = false,
                MonthlyIncome = 3500
            });
    }
}
