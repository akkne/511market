namespace ResaleTelegramBot.Telegram.Services.Abstract;

using Core.Models;
using global::Telegram.Bot;

public interface IListingViewService
{
    Task ShowListingAsync(long userId, int messageId, Listing listing, int listingIndex,
                          int totalListings, Guid? categoryId, string searchText, ITelegramBotClient botClient,
                          CancellationToken cancellationToken);
}