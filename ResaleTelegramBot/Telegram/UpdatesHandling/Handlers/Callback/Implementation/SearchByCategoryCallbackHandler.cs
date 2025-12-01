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
        if (callbackQuery.Data == null)
        {
            _logger.LogWarning("Callback data is null");
            return;
        }

        string data = callbackQuery.Data;
        _logger.LogInformation("SearchByCategoryCallbackHandler handling callback: {Data} for user {UserId}", data,
            callbackQuery.From.Id);

        try
        {
            if (_callbackGenerator.GetCallbackRegexOnSelectCategoryForSearch().IsMatch(data))
            {
                _logger.LogInformation("Matched SelectCategoryForSearch pattern");
                await HandleSelectCategoryForSearchAsync(callbackQuery, botClient, cancellationToken);
                return;
            }

            if (_callbackGenerator.GetCallbackRegexOnViewListing().IsMatch(data))
            {
                _logger.LogInformation("Matched ViewListing pattern");
                await HandleViewListingAsync(callbackQuery, botClient, cancellationToken);
                return;
            }

            _logger.LogWarning("Callback data doesn't match any known pattern: {Data}", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling callback for user {UserId}, data: {Data}",
                callbackQuery.From.Id, data);
        }
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

        _logger.LogInformation(
            "Parsed callback data - CategoryId: {CategoryId}, SearchText: {SearchText}, Index: {Index}, Total: {Total}",
            categoryIdString, searchText, listingIndexString, totalListingsString);

        if (!int.TryParse(listingIndexString, out int startIndex) ||
            !int.TryParse(totalListingsString, out int totalListings))
        {
            _logger.LogWarning("Invalid listing index or total listings. Index: {Index}, Total: {Total}",
                listingIndexString, totalListingsString);
            return;
        }

        Guid? categoryId = null;
        if (Guid.TryParse(categoryIdString, out Guid parsedCategoryId))
        {
            categoryId = parsedCategoryId;
        }
        else if (categoryIdString != "null")
        {
            _logger.LogWarning("Invalid category GUID format: {CategoryId}. Treating as null.", categoryIdString);
            categoryId = null;
        }

        List<Listing> allListings =
            await _listingSearchService.GetListingsAsync(categoryId, searchText, cancellationToken);

        _logger.LogInformation("Found {Count} listings for user {UserId}", allListings.Count, callbackQuery.From.Id);

        if (allListings.Count == 0 || startIndex < 1 || startIndex > allListings.Count)
        {
            _logger.LogWarning("Invalid listing index or no listings found. Index: {Index}, Count: {Count}",
                startIndex, allListings.Count);
            return;
        }

        int listingsToShow = Math.Min(4, allListings.Count - startIndex + 1);
        List<Listing> listingsToDisplay = allListings.GetRange(startIndex - 1, listingsToShow);

        _logger.LogInformation("Displaying {Count} listings starting from index {StartIndex} for user {UserId}",
            listingsToDisplay.Count, startIndex, callbackQuery.From.Id);

        ListingViewSceneContext context = await _sceneStorage.GetOrCreateSceneContextAsync(
            callbackQuery.From.Id,
            "ListingView",
            ListingViewSceneContext.CreateEmpty,
            cancellationToken);

        _logger.LogInformation("Context loaded - MediaGroupMessageId: {MediaId}, ButtonsMessageId: {ButtonsId}",
            context.MediaGroupMessageId, context.ButtonsMessageId);

        try
        {
            ListingViewResult result = await _listingViewService.ShowShortListingAsync(
                callbackQuery.From.Id,
                context.MediaGroupMessageId,
                context.ButtonsMessageId,
                listingsToDisplay, startIndex, totalListings, categoryId, searchText, botClient, cancellationToken);

            _logger.LogInformation("Service returned - MediaGroupMessageId: {MediaId}, ButtonsMessageId: {ButtonsId}",
                result.MediaGroupMessageId, result.ButtonsMessageId);

            context.MediaGroupMessageId = result.MediaGroupMessageId;

            if (result.ButtonsMessageId.HasValue) context.ButtonsMessageId = result.ButtonsMessageId.Value;

            await _sceneStorage.SaveSceneContextAsync(callbackQuery.From.Id, "ListingView", context, cancellationToken);

            _logger.LogInformation("Successfully updated listing view for user {UserId}", callbackQuery.From.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling view listing callback for user {UserId}", callbackQuery.From.Id);
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
            ListingViewSceneContext.CreateEmpty,
            cancellationToken);

        ListingViewResult result = await _listingViewService.ShowShortListingAsync(
            callbackQuery.From.Id,
            context.MediaGroupMessageId,
            context.ButtonsMessageId,
            listingsToDisplay, 1, allListings.Count, categoryGuid, string.Empty, botClient, cancellationToken);

        context.MediaGroupMessageId = result.MediaGroupMessageId;

        if (result.ButtonsMessageId.HasValue) context.ButtonsMessageId = result.ButtonsMessageId.Value;

        await _sceneStorage.SaveSceneContextAsync(callbackQuery.From.Id, "ListingView", context, cancellationToken);
    }
}