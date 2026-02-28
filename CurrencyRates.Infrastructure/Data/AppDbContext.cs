using CurrencyRates.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CurrencyRates.Infrastructure.Data;

/// <summary>
/// Контекст бази даних. Використовується ТІЛЬКИ для міграцій EF Core.
/// Всі реальні запити до БД виконуються через SqlKata.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<CurrencyRate> CurrencyRates { get; set; }
    public DbSet<Currency> Currencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- Таблиця Currencies ---
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("Currencies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(30);

            // Унікальний індекс — не може бути двох однакових кодів валюти
            entity.HasIndex(e => e.Code).IsUnique();

            // Заповнюємо довідник одразу при міграції
            entity.HasData(
                new Currency { Id = 1, Code = "USD", Name = "Долар США" },
                new Currency { Id = 2, Code = "EUR", Name = "Євро" },
                new Currency { Id = 3, Code = "DKK", Name = "Данська крона" },
                new Currency { Id = 4, Code = "PLN", Name = "Злотий" }
            );
        });

        // --- Таблиця CurrencyRates ---
        modelBuilder.Entity<CurrencyRate>(entity =>
        {
            entity.ToTable("CurrencyRates");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Rate)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            // Зберігаємо "Auto"/"Manual" в БД як текст, а не 0/1
            entity.Property(e => e.Source)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(10);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Зовнішній ключ на Currencies
            entity.HasOne(e => e.Currency)
                .WithMany()
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Унікальний індекс — не може бути двох курсів для однієї валюти на одну дату
            entity.HasIndex(e => new { e.CurrencyId, e.RateDate }).IsUnique();
        });
    }
}