namespace ResaleTelegramBot.Core.Models;

public class User
{
    public Guid Id { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }
    public SellerProfile? SellerProfile { get; set; }
    public DateTime CreatedAt { get; set; }
}