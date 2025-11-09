namespace ResaleTelegramBot.Core.Models;

public class BuyerProfile
{
    public User UserProfile { get; set; }
    public Guid UserProfileId { get; set; }
    public List<Favorite> Favorites { get; set; }
}