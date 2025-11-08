namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Core.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ListingEntityTypeConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasMany(x => x.Photos)
               .WithOne(x => x.Listing)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Status)
               .HasConversion(
                    x => x.ToString(),
                    x => Enum.Parse<ListingStatus>(x)
                )
               .IsRequired();

        builder.HasOne(x => x.Category)
               .WithMany(x => x.Listings)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Description).IsRequired();
    }
}