namespace ResaleTelegramBot.Persistence.DbContexts.Seeders;

using Core.Models;
using Microsoft.EntityFrameworkCore;

public static class AddDefaultCategoriesSeeder
{
    public static ModelBuilder SeedCategoryData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = Guid.Parse("c1111111-1111-1111-1111-111111111111"),
                Name = "Верх"
            },
            new Category
            {
                Id = Guid.Parse("c2222222-2222-2222-2222-222222222222"),
                Name = "Низ"
            },
            new Category
            {
                Id = Guid.Parse("c3333333-3333-3333-3333-333333333333"),
                Name = "Обувь"
            },
            new Category
            {
                Id = Guid.Parse("c4444444-4444-4444-4444-444444444444"),
                Name = "Головные уборы"
            },
            new Category
            {
                Id = Guid.Parse("c5555555-5555-5555-5555-555555555555"),
                Name = "Аксессуары"
            },
            new Category
            {
                Id = Guid.Parse("c6666666-6666-6666-6666-666666666666"),
                Name = "Нижнее бельё"
            }
        );

        return modelBuilder;
    }
}