namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using ResaleTelegramBot.Services.Abstract;
using Services.Abstract;
using Texts.Output;

public class SearchByCategoryCallbackHandler : ICallbackHandler
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ICategoryService _categoryService;
    private readonly IListingSearchService _listingSearchService;
    private readonly IListingViewService _listingViewService;
    private readonly ILogger<SearchByCategoryCallbackHandler> _logger;

    public SearchByCategoryCallbackHandler(ILogger<SearchByCategoryCallbackHandler> logger,
                                           ICallbackGenerator callbackGenerator,
                                           ICallbackKeyboardGenerator callbackKeyboardGenerator,
                                           ICategoryService categoryService, IListingSearchService listingSearchService,
                                           IListingViewService listingViewService)
    {
        _logger = logger;
        _callbackGenerator = callbackGenerator;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
        _categoryService = categoryService;
        _listingSearchService = listingSearchService;
        _listingViewService = listingViewService;
    }

    public bool CanHandle(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == null) return false;

        string data = callbackQuery.Data;

        return _callbackGenerator.GetCallbackRegexOnSelectCategoryForSearch().IsMatch(data) ||
               _callbackGenerator.GetCallbackRegexOnViewListing().IsMatch(data);
    }

    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                          CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        string data = callbackQuery.Data;

        if (_callbackGenerator.GetCallbackRegexOnSelectCategoryForSearch().IsMatch(data))
        {
            await HandleSelectCategoryForSearchAsync(callbackQuery, botClient, cancellationToken);
            return;
        }


        if (_callbackGenerator.GetCallbackRegexOnViewListing().IsMatch(data))
            await HandleViewListingAsync(callbackQuery, botClient, cancellationToken);
    }


    private async Task HandleViewListingAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                              CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        Match match = _callbackGenerator.GetCallbackRegexOnViewListing().Match(callbackQuery.Data);
        if (!match.Success) return;

        string categoryIdString = match.Groups[CallbackGenerationStaticStrings.CategoryId].Value;
        string searchText = Uri.UnescapeDataString(match.Groups[CallbackGenerationStaticStrings.SearchText].Value);
        string listingIndexString = match.Groups[CallbackGenerationStaticStrings.ListingIndex].Value;
        string totalListingsString = match.Groups[CallbackGenerationStaticStrings.TotalListings].Value;

        if (!int.TryParse(listingIndexString, out int startIndex) ||
            !int.TryParse(totalListingsString, out int totalListings))
        {
            _logger.LogWarning("Invalid listing index or total listings");
            return;
        }

        Guid? categoryId = categoryIdString == "null" ? null : Guid.Parse(categoryIdString);

        List<Listing> allListings =
            await _listingSearchService.GetListingsAsync(categoryId, searchText, cancellationToken);

        if (allListings.Count == 0 || startIndex < 1 || startIndex > allListings.Count)
        {
            _logger.LogWarning("Invalid listing index or no listings found");
            return;
        }

        int listingsToShow = Math.Min(4, allListings.Count - startIndex + 1);
        List<Listing> listingsToDisplay = allListings.GetRange(startIndex - 1, listingsToShow);

        await _listingViewService.ShowShortListingAsync(callbackQuery.From.Id, callbackQuery.Message!.MessageId,
            listingsToDisplay, startIndex, totalListings, categoryId, searchText, botClient, cancellationToken);
    }

    private async Task HandleSelectCategoryForSearchAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                                          CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        Match match = _callbackGenerator.GetCallbackRegexOnSelectCategoryForSearch().Match(callbackQuery.Data);
        if (!match.Success) return;

        string categoryGuidString = match.Groups[CallbackGenerationStaticStrings.CategoryId].Value;
        if (!Guid.TryParse(categoryGuidString, out Guid categoryGuid))
        {
            _logger.LogWarning("Invalid category GUID: {Guid}", categoryGuidString);
            return;
        }

        List<Listing> allListings =
            await _listingSearchService.GetListingsAsync(categoryGuid, string.Empty, cancellationToken);

        if (allListings.Count == 0)
        {
            await botClient.EditMessageText(callbackQuery.From.Id, callbackQuery.Message!.MessageId,
                ResponseMessageStaticTexts.OnNoListingsFound, ParseMode.Html, null,
                cancellationToken: cancellationToken);
            return;
        }

        int listingsToShow = Math.Min(4, allListings.Count);
        List<Listing> listingsToDisplay = allListings.GetRange(0, listingsToShow);

        await _listingViewService.ShowShortListingAsync(callbackQuery.From.Id, callbackQuery.Message!.MessageId,
            listingsToDisplay, 1, allListings.Count, categoryGuid, string.Empty, botClient, cancellationToken);
    }
}