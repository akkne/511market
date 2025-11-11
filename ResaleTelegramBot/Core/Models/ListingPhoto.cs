namespace ResaleTelegramBot.Core.Models;

using Services.Contracts.Listing.Models;

public class ListingPhoto
{
    public Guid Id { get; set; }
    public Listing Listing { get; set; }
    public string TelegramFileId { get; set; }
    public int Order { get; set; }
    public DateTime AddedAt { get; set; }

    public static List<ListingPhoto> CreateList(List<ListingPhotosModel> photos)
    {
        return photos.Select((t, i)
            => new ListingPhoto
            {
                Id = Guid.NewGuid(), TelegramFileId = t.TelegramFileId, Order = i, AddedAt = DateTime.UtcNow
            }).ToList();
    }
}