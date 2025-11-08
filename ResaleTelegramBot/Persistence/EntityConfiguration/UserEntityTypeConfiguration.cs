namespace ResaleTelegramBot.Persistence.EntityConfiguration;

using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder.OwnsOne(x => x.TelegramData, data => 
        {
            data.Property(x => x.Id).IsRequired();
            data.Property(x => x.Username).IsRequired();
        });
    }
}