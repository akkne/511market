namespace ResaleTelegramBot.Core.Models;

using Services.Contracts.Listing.Models;
using Shared.Enums;

public class Listing
{
    public Guid Id { get; set; }
    public SellerProfile SellerProfile { get; set; }
    public string Title { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Favorite> FavoritedBy { get; set; }
    public List<ListingPhoto> Photos { get; set; }

    public static Listing Create(string title, string description, decimal price, SellerProfile seller,
                                 Category category, List<ListingPhotosModel> photos)
    {
        return new Listing
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            Price = price,
            Category = category,
            CreatedAt = DateTime.UtcNow,
            FavoritedBy = [],
            SellerProfile = seller,
            Status = ListingStatus.Active,
            Photos = ListingPhoto.CreateList(photos)
        };
    }
}