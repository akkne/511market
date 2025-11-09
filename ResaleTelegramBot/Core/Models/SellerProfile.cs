namespace ResaleTelegramBot.Core.Models;

public class SellerProfile
{
    public User UserProfile { get; set; }
    public Guid UserProfileId { get; set; }
    public List<Listing> Listings { get; set; }
}