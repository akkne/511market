namespace ResaleTelegramBot.Telegram.Helpers.Abstract;

using Core.Models;
using global::Telegram.Bot.Types.ReplyMarkups;

public interface ICallbackKeyboardGenerator
{
    InlineKeyboardMarkup GenerateOnConfirmListingPublication();
    InlineKeyboardMarkup GenerateOnChoosingCategoryOnAddingListing(List<Category> categories);
}