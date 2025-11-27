namespace ResaleTelegramBot.Telegram.Helpers.Abstract;

using Core.Models;
using global::Telegram.Bot.Types.ReplyMarkups;

public interface ICallbackKeyboardGenerator
{
    InlineKeyboardMarkup GenerateOnConfirmListingPublication();
    InlineKeyboardMarkup GenerateOnChoosingCategoryOnAddingListing(List<Category> categories);
    InlineKeyboardMarkup GenerateOnFinishPhotoUploading();
    InlineKeyboardMarkup GenerateOnSearchTypeSelection();
    InlineKeyboardMarkup GenerateOnCategorySelectionForSearch(List<Category> categories);

    InlineKeyboardMarkup GenerateOnShortListingCard(List<Listing> listings, int startIndex, int totalListings,
                                                    Guid? categoryId, string searchText);
}