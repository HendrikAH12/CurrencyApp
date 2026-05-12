using CurrencyApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CurrencyApp.Infra.Context.Configurations;

public class UserCurrencyConfiguration : IEntityTypeConfiguration<UserCurrency>
{
    public void Configure(EntityTypeBuilder<UserCurrency> builder)
    {
        builder.ToTable("UserCurrencies");

        builder.HasKey(x => new { x.UserId, x.CurrencyId });

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.HasOne<User>()
            .WithMany(x => x.Holdings)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
