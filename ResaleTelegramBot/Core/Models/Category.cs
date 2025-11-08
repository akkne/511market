namespace ResaleTelegramBot.Core.Models;

public class Category
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Listing> Listings { get; set; }
}