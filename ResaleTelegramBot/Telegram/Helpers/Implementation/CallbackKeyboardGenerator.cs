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

    public InlineKeyboardMarkup GenerateOnFinishPhotoUploading()
    {
        return new InlineKeyboardMarkup(
            new InlineKeyboardButton(CallbackKeyboardStaticTexts.OnFinishPhotoUploading)
            {
                CallbackData = _callbackGenerator.GenerateCallbackRegexOnFinishPhotoUploading()
            });
    }

    public InlineKeyboardMarkup GenerateOnSearchTypeSelection()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                new InlineKeyboardButton(CallbackKeyboardStaticTexts.SearchByCategory)
                {
                    CallbackData = _callbackGenerator.GenerateCallbackRegexOnSearchByCategory()
                }
            },
            new[]
            {
                new InlineKeyboardButton(CallbackKeyboardStaticTexts.SearchByText)
                {
                    CallbackData = _callbackGenerator.GenerateCallbackRegexOnSearchByText()
                }
            }
        });
    }

    public InlineKeyboardMarkup GenerateOnCategorySelectionForSearch(List<Category> categories)
    {
        List<Category> categoriesToShow = categories.Take(6).ToList();

        List<List<InlineKeyboardButton>> rows = [];
        for (int i = 0; i < categoriesToShow.Count; i += 3)
        {
            List<InlineKeyboardButton> row = [];
            for (int j = i; j < i + 3 && j < categoriesToShow.Count; j++)
                row.Add(new InlineKeyboardButton(categoriesToShow[j].Name)
                {
                    CallbackData =
                        _callbackGenerator.GenerateCallbackRegexOnSelectCategoryForSearch(categoriesToShow[j].Id
                           .ToString())
                });
            rows.Add(row);
        }

        return new InlineKeyboardMarkup(rows);
    }

    public InlineKeyboardMarkup GenerateOnShortListingCard(Listing listing, int listingIndex,
                                                           int totalListings, Guid? categoryId, string searchText)
    {
        List<List<InlineKeyboardButton>> rows = [];

        rows.Add([
            new InlineKeyboardButton(CallbackKeyboardStaticTexts.ReportListing)
            {
                CallbackData = _callbackGenerator.GenerateCallbackRegexOnReportListing(listing.Id)
            }
        ]);

        if (totalListings > 1)
        {
            List<InlineKeyboardButton> navigationRow = [];
            if (listingIndex > 1)
            {
                int prevIndex = listingIndex - 1;
                navigationRow.Add(new InlineKeyboardButton("◀️ Назад")
                {
                    CallbackData = _callbackGenerator.GenerateCallbackRegexOnViewListing(categoryId, searchText,
                        prevIndex, totalListings)
                });
            }

            if (listingIndex < totalListings)
            {
                int nextIndex = listingIndex + 1;
                navigationRow.Add(new InlineKeyboardButton("Вперёд ▶️")
                {
                    CallbackData = _callbackGenerator.GenerateCallbackRegexOnViewListing(categoryId, searchText,
                        nextIndex, totalListings)
                });
            }

            if (navigationRow.Count > 0) rows.Add(navigationRow);
        }

        return new InlineKeyboardMarkup(rows);
    }
}