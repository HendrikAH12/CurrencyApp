using CurrencyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyApp.Infra.Context.Configurations;

public class ExchangeRateCacheConfiguration : IEntityTypeConfiguration<ExchangeRateCache>
{
    public void Configure(EntityTypeBuilder<ExchangeRateCache> builder)
    {
        builder.ToTable("ExchangeRateCaches");

        builder.HasKey(x => new { x.FromCode, x.ToCode });

        builder.Property(x => x.FromCode)
            .IsRequired()
            .HasMaxLength(3)
            .IsUnicode(false);
        
        builder.Property(x => x.ToCode)
            .IsRequired()
            .HasMaxLength(3)
            .IsUnicode(false);

        builder.Property(x => x.Rate)
            .HasPrecision(18, 8);

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();
    }
}
