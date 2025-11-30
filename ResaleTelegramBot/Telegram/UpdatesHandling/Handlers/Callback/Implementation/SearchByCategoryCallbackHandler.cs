namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using Persistence.Scenes.Abstract;
using ResaleTelegramBot.Services.Abstract;
using Scenes.Contexts.Implementation;
using Services.Abstract;
using Services.Models;
using Texts.Output;

public class SearchByCategoryCallbackHandler : ICallbackHandler
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ICategoryService _categoryService;
    private readonly IListingSearchService _listingSearchService;
    private readonly IListingViewService _listingViewService;
    private readonly ILogger<SearchByCategoryCallbackHandler> _logger;
    private readonly ISceneStorage _sceneStorage;

    public SearchByCategoryCallbackHandler(ILogger<SearchByCategoryCallbackHandler> logger,
                                           ICallbackGenerator callbackGenerator,
                                           ICallbackKeyboardGenerator callbackKeyboardGenerator,
                                           ICategoryService categoryService, IListingSearchService listingSearchService,
                                           IListingViewService listingViewService, ISceneStorage sceneStorage)
    {
        _logger = logger;
        _callbackGenerator = callbackGenerator;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
        _categoryService = categoryService;
        _listingSearchService = listingSearchService;
        _listingViewService = listingViewService;
        _sceneStorage = sceneStorage;
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
        if (callbackQuery.Data == null)
        {
            _logger.LogWarning("Callback data is null");
            return;
        }

        Match match = _callbackGenerator.GetCallbackRegexOnViewListing().Match(callbackQuery.Data);
        if (!match.Success)
        {
            _logger.LogWarning("Callback data doesn't match view listing pattern: {Data}", callbackQuery.Data);
            return;
        }

        string categoryIdString = match.Groups[CallbackGenerationStaticStrings.CategoryId].Value;
        string searchText = Uri.UnescapeDataString(match.Groups[CallbackGenerationStaticStrings.SearchText].Value);
        string listingIndexString = match.Groups[CallbackGenerationStaticStrings.ListingIndex].Value;
        string totalListingsString = match.Groups[CallbackGenerationStaticStrings.TotalListings].Value;

        if (!int.TryParse(listingIndexString, out int startIndex) ||
            !int.TryParse(totalListingsString, out int totalListings))
        {
            _logger.LogWarning("Invalid listing index or total listings. Index: {Index}, Total: {Total}",
                listingIndexString, totalListingsString);
            return;
        }

        Guid? categoryId = null;
        if (categoryIdString != "null" && Guid.TryParse(categoryIdString, out Guid parsedCategoryId))
        {
            categoryId = parsedCategoryId;
        }
        else if (categoryIdString != "null")
        {
            _logger.LogWarning("Invalid category GUID format: {CategoryId}", categoryIdString);
            return;
        }

        List<Listing> allListings =
            await _listingSearchService.GetListingsAsync(categoryId, searchText, cancellationToken);

        if (allListings.Count == 0 || startIndex < 1 || startIndex > allListings.Count)
        {
            _logger.LogWarning("Invalid listing index or no listings found. Index: {Index}, Count: {Count}",
                startIndex, allListings.Count);
            return;
        }

        int listingsToShow = Math.Min(4, allListings.Count - startIndex + 1);
        List<Listing> listingsToDisplay = allListings.GetRange(startIndex - 1, listingsToShow);

        ListingViewSceneContext context = await _sceneStorage.GetOrCreateSceneContextAsync(
            callbackQuery.From.Id,
            "ListingView",
            () => new ListingViewSceneContext(),
            cancellationToken);

        try
        {
            ListingViewResult result = await _listingViewService.ShowShortListingAsync(
                callbackQuery.From.Id,
                context.MediaGroupMessageId == 0 ? null : context.MediaGroupMessageId,
                context.ButtonsMessageId == 0 ? null : context.ButtonsMessageId,
                listingsToDisplay, startIndex, totalListings, categoryId, searchText, botClient, cancellationToken);

            if (result.MediaGroupMessageId.HasValue) context.MediaGroupMessageId = result.MediaGroupMessageId.Value;

            if (result.ButtonsMessageId.HasValue) context.ButtonsMessageId = result.ButtonsMessageId.Value;

            await _sceneStorage.SaveSceneContextAsync(callbackQuery.From.Id, "ListingView", context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling view listing callback for user {UserId}", callbackQuery.From.Id);
            throw;
        }
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
                ResponseMessageStaticTexts.OnNoListingsFound, ParseMode.Html,
                cancellationToken: cancellationToken);
            return;
        }

        int listingsToShow = Math.Min(4, allListings.Count);
        List<Listing> listingsToDisplay = allListings.GetRange(0, listingsToShow);

        ListingViewSceneContext context = await _sceneStorage.CreateSceneContextAsync(
            callbackQuery.From.Id,
            "ListingView",
            () => new ListingViewSceneContext(),
            cancellationToken);

        ListingViewResult result = await _listingViewService.ShowShortListingAsync(
            callbackQuery.From.Id,
            context.MediaGroupMessageId == 0 ? null : context.MediaGroupMessageId,
            context.ButtonsMessageId == 0 ? null : context.ButtonsMessageId,
            listingsToDisplay, 1, allListings.Count, categoryGuid, string.Empty, botClient, cancellationToken);

        if (result.MediaGroupMessageId.HasValue) context.MediaGroupMessageId = result.MediaGroupMessageId.Value;

        if (result.ButtonsMessageId.HasValue) context.ButtonsMessageId = result.ButtonsMessageId.Value;

        await _sceneStorage.SaveSceneContextAsync(callbackQuery.From.Id, "ListingView", context, cancellationToken);
    }
}