namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ListingPhotoEntityTypeConfiguration : IEntityTypeConfiguration<ListingPhoto>
{
    public void Configure(EntityTypeBuilder<ListingPhoto> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.TelegramFileId).IsRequired();

        builder.Property(x => x.Order).IsRequired();
    }
}