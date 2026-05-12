using CurrencyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyApp.Infra.Context.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(3)
            .IsUnicode(false);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(x => x.Code)
            .IsUnique();
    }
}
