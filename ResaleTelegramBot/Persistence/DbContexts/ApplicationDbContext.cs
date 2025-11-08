namespace ResaleTelegramBot.Persistence.DbContexts;

using Core.Models;
using Microsoft.EntityFrameworkCore;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.Migrate();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Listing> Listings { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ListingPhoto> ListingPhotos { get; set; }
    public DbSet<SellerProfile> SellerProfiles { get; set; }
    public DbSet<BuyerProfile> BuyerProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}