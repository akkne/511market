namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using ResaleTelegramBot.Services.Abstract;
using Services.Abstract;

public class ToggleFavoriteCallbackHandler : ICallbackHandler
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly IFavoriteService _favoriteService;
    private readonly IListingService _listingService;
    private readonly IListingViewService _listingViewService;
    private readonly ILogger<ToggleFavoriteCallbackHandler> _logger;

    public ToggleFavoriteCallbackHandler(ILogger<ToggleFavoriteCallbackHandler> logger,
                                         ICallbackGenerator callbackGenerator,
                                         IFavoriteService favoriteService,
                                         IListingService listingService,
                                         IListingViewService listingViewService)
    {
        _logger = logger;
        _callbackGenerator = callbackGenerator;
        _favoriteService = favoriteService;
        _listingService = listingService;
        _listingViewService = listingViewService;
    }

    public bool CanHandle(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == null) return false;

        string data = callbackQuery.Data;
        return _callbackGenerator.GetCallbackRegexOnToggleFavorite().IsMatch(data);
    }

    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                          CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null)
        {
            _logger.LogWarning("Callback data is null");
            return;
        }

        Match match = _callbackGenerator.GetCallbackRegexOnToggleFavorite().Match(callbackQuery.Data);
        if (!match.Success)
        {
            _logger.LogWarning("Callback data doesn't match toggle favorite pattern: {Data}", callbackQuery.Data);
            return;
        }

        string listingIdString = match.Groups[CallbackGenerationStaticStrings.ListingId].Value;
        string messageIdString = match.Groups["messageId"].Value;

        _logger.LogInformation("Parsed toggle favorite callback data - ListingId: {ListingId}, MessageId: {MessageId}",
            listingIdString, messageIdString);

        if (!Guid.TryParse(listingIdString, out Guid listingId))
        {
            _logger.LogWarning("Invalid listing GUID format: {ListingId}", listingIdString);
            return;
        }

        if (!int.TryParse(messageIdString, out int messageId))
        {
            _logger.LogWarning("Invalid message ID format: {MessageId}", messageIdString);
            return;
        }

        bool isFavorite = await _favoriteService.IsFavoriteAsync(callbackQuery.From.Id, listingId, cancellationToken);

        if (isFavorite)
        {
            await _favoriteService.RemoveFavoriteAsync(callbackQuery.From.Id, listingId, cancellationToken);
            _logger.LogInformation("Removed listing {ListingId} from favorites for user {UserId}", listingId,
                callbackQuery.From.Id);
        }
        else
        {
            await _favoriteService.AddFavoriteAsync(callbackQuery.From.Id, listingId, cancellationToken);
            _logger.LogInformation("Added listing {ListingId} to favorites for user {UserId}", listingId,
                callbackQuery.From.Id);
        }

        Listing? listing = await _listingService.GetListingByIdAsync(listingId, cancellationToken);
        if (listing == null)
        {
            _logger.LogWarning("Listing not found with id: {ListingId}", listingId);
            return;
        }

        bool newFavoriteStatus = !isFavorite;

        try
        {
            await _listingViewService.UpdateLongListingMessageAsync(
                callbackQuery.From.Id,
                messageId,
                listing,
                newFavoriteStatus,
                botClient,
                cancellationToken);

            _logger.LogInformation("Successfully updated long listing message for user {UserId}",
                callbackQuery.From.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating long listing message for user {UserId}", callbackQuery.From.Id);
        }
    }
}