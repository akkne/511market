namespace ResaleTelegramBot.Core.Models;

public class SellerProfile
{
    public Guid Id { get; set; }
    public User UserProfile { get; set; }
    public List<Listing> Listings { get; set; }
    public DateTime CreatedAt { get; set; }
}