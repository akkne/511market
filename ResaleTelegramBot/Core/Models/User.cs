namespace ResaleTelegramBot.Core.Models;

using Shared.Models;

public class User
{
    public Guid Id { get; set; }
    public TelegramData TelegramData { get; set; }
    public BuyerProfile? BuyerProfile { get; set; }
    public SellerProfile? SellerProfile { get; set; }
    public DateTime CreatedAt { get; set; }
}