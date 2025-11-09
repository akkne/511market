namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SellerProfileEntityTypeConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasOne(x => x.UserProfile)
               .WithOne(x => x.SellerProfile)
               .HasForeignKey<SellerProfile>(x => x.UserProfileId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.UserProfileId)
               .IsRequired();

        builder.HasMany(x => x.Listings)
               .WithOne(x => x.SellerProfile)
               .OnDelete(DeleteBehavior.Cascade);
    }
}