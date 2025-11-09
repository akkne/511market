namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class BuyerProfileEntityTypeConfiguration : IEntityTypeConfiguration<BuyerProfile>
{
    public void Configure(EntityTypeBuilder<BuyerProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasOne(x => x.UserProfile)
               .WithOne(x => x.BuyerProfile)
               .HasForeignKey<BuyerProfile>(x => x.UserProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.UserProfileId)
               .IsRequired();
    }
}