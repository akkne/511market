namespace ResaleTelegramBot.Core.Models;

public class Favorite
{
    public BuyerProfile BuyerProfile { get; set; }
    public Listing Listing { get; set; }

    public DateTime CreatedAt { get; set; }
}