namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot.Types.ReplyMarkups;
using Texts.Input;

public class CallbackKeyboardGenerator : ICallbackKeyboardGenerator
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ILogger<CallbackKeyboardGenerator> _logger;

    public CallbackKeyboardGenerator(ICallbackGenerator callbackGenerator, ILogger<CallbackKeyboardGenerator> logger)
    {
        _callbackGenerator = callbackGenerator;
        _logger = logger;
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

    public InlineKeyboardMarkup GenerateOnShortListingCard(List<Listing> listings, int startIndex,
                                                           int totalListings, Guid? categoryId, string searchText)
    {
        List<List<InlineKeyboardButton>> rows = [];

        List<InlineKeyboardButton> numberButtonsRow = [];
        for (int i = 0; i < listings.Count && i < 4; i++)
        {
            Listing listing = listings[i];
            if (listing.Id == Guid.Empty) continue;

            int buttonNumber = i + 1;
            string callbackData = _callbackGenerator.GenerateCallbackRegexOnViewLongListing(listing.Id,
                searchText, startIndex, totalListings);

            if (callbackData.Length > 64)
            {
                _logger.LogWarning("CallbackData.Length > 64, callback data wasn't added");
                continue;
            }

            numberButtonsRow.Add(new InlineKeyboardButton(buttonNumber.ToString())
            {
                CallbackData = callbackData
            });
        }

        if (numberButtonsRow.Count > 0) rows.Add(numberButtonsRow);

        List<InlineKeyboardButton> navigationRow = [];
        if (startIndex > 1)
        {
            int prevStartIndex = Math.Max(1, startIndex - 4);
            string prevCallbackData = _callbackGenerator.GenerateCallbackRegexOnViewListing(categoryId, searchText,
                prevStartIndex, totalListings);

            if (prevCallbackData.Length <= 64)
                navigationRow.Add(new InlineKeyboardButton("◀️ Назад")
                {
                    CallbackData = prevCallbackData
                });
        }

        int actualListingsShown = Math.Min(listings.Count, 4);
        if (startIndex + actualListingsShown <= totalListings)
        {
            int nextStartIndex = startIndex + actualListingsShown;
            string nextCallbackData = _callbackGenerator.GenerateCallbackRegexOnViewListing(categoryId, searchText,
                nextStartIndex, totalListings);

            if (nextCallbackData.Length <= 64)
                navigationRow.Add(new InlineKeyboardButton("Вперёд ▶️")
                {
                    CallbackData = nextCallbackData
                });
        }

        if (navigationRow.Count > 0) rows.Add(navigationRow);

        return new InlineKeyboardMarkup(rows);
    }
}