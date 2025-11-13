namespace ResaleTelegramBot.Services.Abstract;

public interface IFavoriteService
{
    Task<bool> AddFavoriteAsync(long telegramUserId, Guid listingId, CancellationToken cancellationToken = default);
    Task<bool> RemoveFavoriteAsync(long telegramUserId, Guid listingId, CancellationToken cancellationToken = default);
    Task<bool> IsFavoriteAsync(long telegramUserId, Guid listingId, CancellationToken cancellationToken = default);
}