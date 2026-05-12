using CurrencyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyApp.Infra.Context.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasOne(x => x.MainCurrency)
            .WithMany()
            .HasForeignKey(x => x.MainCurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Holdings)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
