namespace ResaleTelegramBot.Telegram.Services.Abstract;

using Core.Models;
using global::Telegram.Bot;
using Models;

public interface IListingViewService
{
    Task<ListingViewResult> ShowShortListingAsync(long userId, List<int> mediaGroupMessageIdList,
                                                  int? buttonsMessageId, List<Listing> listings,
                                                  int startIndex, int totalListings, Guid? categoryId,
                                                  string searchText, ITelegramBotClient botClient,
                                                  CancellationToken cancellationToken);
}