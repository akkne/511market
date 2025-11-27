namespace ResaleTelegramBot.Telegram.Services.Abstract;

using Core.Models;
using global::Telegram.Bot;

public interface IListingViewService
{
    Task ShowShortListingAsync(long userId, int messageId, List<Listing> listings, int startIndex,
                               int totalListings, Guid? categoryId, string searchText, ITelegramBotClient botClient,
                               CancellationToken cancellationToken);
}