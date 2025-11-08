namespace ResaleTelegramBot.Core.Models;

using Shared;

public class Listing
{
    public Guid Id { get; set; }
    public SellerProfile SellerProfile { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public Category Category { get; set; }
    public ListingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Favorite> FavoritedBy { get; set; }
    public List<ListingPhoto> Photos { get; set; }
}