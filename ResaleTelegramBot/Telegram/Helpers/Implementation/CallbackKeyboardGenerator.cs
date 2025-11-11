namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot.Types.ReplyMarkups;
using Texts.Input;

public class CallbackKeyboardGenerator : ICallbackKeyboardGenerator
{
    private readonly ICallbackGenerator _callbackGenerator;

    public CallbackKeyboardGenerator(ICallbackGenerator callbackGenerator)
    {
        _callbackGenerator = callbackGenerator;
    }

    public InlineKeyboardMarkup GenerateOnConfirmListingPublication()
    {
        return new InlineKeyboardMarkup(
            new InlineKeyboardButton(CallbackKeyboardStaticTexts.OnListingPublicationConfirm)
            {
                CallbackData = _callbackGenerator.GenerateCallbackRegexOnConfirmListingPublication()
            });
    }

    public InlineKeyboardMarkup GenerateOnChoosingCategoryOnAddingListing(List<Category> categories)
    {
        List<Category> categoriesToShow = categories[..6];

        List<List<InlineKeyboardButton>> rows = [];
        for (int i = 0; i < categoriesToShow.Count; i += 3)
        {
            List<InlineKeyboardButton> row = [];
            for (int j = i; j < i + 3; j++)
                row.Add(new InlineKeyboardButton(categoriesToShow[j].Name)
                {
                    CallbackData =
                        _callbackGenerator.GenerateCallbackRegexOnChoosingCategoryOnAddingListing(categoriesToShow[j].Id
                           .ToString())
                });
            rows.Add(row);
        }

        return new InlineKeyboardMarkup(rows);
    }
}