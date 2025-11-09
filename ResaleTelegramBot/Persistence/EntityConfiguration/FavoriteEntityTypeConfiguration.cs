namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FavoriteEntityTypeConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.HasOne(f => f.BuyerProfile)
               .WithMany(bp => bp.Favorites)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Listing)
               .WithMany(l => l.FavoritedBy)
               .OnDelete(DeleteBehavior.Cascade);
    }
}