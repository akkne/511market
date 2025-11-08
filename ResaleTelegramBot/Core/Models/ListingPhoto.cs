namespace ResaleTelegramBot.Core.Models;

public class ListingPhoto
{
    public Guid Id { get; set; }
    public Listing Listing { get; set; }
    public string TelegramFileId { get; set; }
    public DateTime AddedAt { get; set; }
}