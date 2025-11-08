namespace ResaleTelegramBot.Core.Models;

public class BuyerProfile
{
    public Guid Id { get; set; }
    public User UserProfile { get; set; }
    public List<Favorite> Favorites { get; set; }
}