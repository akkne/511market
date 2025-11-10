namespace ResaleTelegramBot.Services.Contracts.Listing;

using Models;

public class AddListingContract
{
    public long TelegramUserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public Guid CategoryId { get; set; }
    public List<ListingPhotosModel> Photos { get; set; }

    public static AddListingContract Create(long telegramUserId, string name, string description, decimal price,
                                            Guid categoryId,
                                            List<ListingPhotosModel> photos)
    {
        return new AddListingContract
        {
            TelegramUserId = telegramUserId,
            Name = name,
            CategoryId = categoryId,
            Description = description,
            Price = price,
            Photos = photos
        };
    }
}