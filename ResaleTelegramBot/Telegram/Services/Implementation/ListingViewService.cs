namespace ResaleTelegramBot.Telegram.Services.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Exceptions;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;

public class ListingViewService : IListingViewService
{
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ILogger<ListingViewService> _logger;

    public ListingViewService(ILogger<ListingViewService> logger,
                              ICallbackKeyboardGenerator callbackKeyboardGenerator)
    {
        _logger = logger;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
    }

    public async Task ShowShortListingAsync(long userId, int messageId, List<Listing> listings, int startIndex,
                                            int totalListings, Guid? categoryId, string searchText,
                                            ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        if (listings.Count == 0)
        {
            _logger.LogWarning("Empty listings list");
            return;
        }

        if (startIndex < 1 || startIndex > totalListings)
        {
            _logger.LogWarning("Invalid start index: {Index}, total: {Total}", startIndex, totalListings);
            return;
        }

        string cardText = FormatListingsCard(listings, startIndex, totalListings);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnShortListingCard(listings, startIndex, totalListings, categoryId,
                searchText);

        List<ListingPhoto> photos = GetFirstPhotosFromListings(listings);
        if (photos.Count == 0)
        {
            try
            {
                await botClient.EditMessageText(userId, messageId, cardText, ParseMode.Html,
                    keyboardMarkup, cancellationToken: cancellationToken);
            }
            catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                _logger.LogInformation("Message content unchanged for user {UserId}, message {MessageId}", userId,
                    messageId);
            }

            return;
        }

        if (photos.Count == 1)
        {
            string photoFileId = photos[0].TelegramFileId;
            try
            {
                await botClient.EditMessageMedia(new ChatId(userId), messageId,
                    new InputMediaPhoto(photoFileId)
                    {
                        Caption = cardText, ParseMode = ParseMode.Html
                    }, keyboardMarkup, cancellationToken: cancellationToken);
            }
            catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                _logger.LogInformation("Message content unchanged for user {UserId}, message {MessageId}", userId,
                    messageId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to edit message media for user {UserId}, message {MessageId}. Deleting and resending.",
                    userId, messageId);
                await DeleteAndSendSinglePhotoAsync(userId, messageId, photos[0], cardText, keyboardMarkup, botClient,
                    cancellationToken);
            }
        }
        else
        {
            await DeleteAndSendMediaGroupAsync(userId, messageId, photos, cardText, keyboardMarkup, botClient,
                cancellationToken);
        }
    }

    public async Task ShowLongListingAsync()
    {
        throw new NotImplementedException();
    }

    private static string FormatListingsCard(List<Listing> listings, int startIndex, int totalListings)
    {
        List<string> listingTexts = [];
        for (int i = 0; i < listings.Count && i < 4; i++)
        {
            Listing listing = listings[i];
            listingTexts.Add($"""
                              {i}. <b>{listing.Title}</b>:
                              💰 {listing.Price} ₽ss
                              📂 {listing.Category.Name}
                              """);
        }

        return string.Join("\n\n", listingTexts);
    }

    private static bool HasButtons(InlineKeyboardMarkup keyboardMarkup)
    {
        if (keyboardMarkup?.InlineKeyboard == null) return false;

        return keyboardMarkup.InlineKeyboard.Any(row => row != null && row.Any());
    }

    private static List<ListingPhoto> GetFirstPhotosFromListings(List<Listing> listings)
    {
        List<ListingPhoto> photos = [];
        foreach (Listing listing in listings)
        {
            if (listing.Photos == null || listing.Photos.Count == 0) continue;

            ListingPhoto? firstPhoto = listing.Photos.OrderBy(p => p.Order).FirstOrDefault();
            if (firstPhoto != null) photos.Add(firstPhoto);
        }

        return photos;
    }

    private async Task DeleteAndSendSinglePhotoAsync(long userId, int messageId, ListingPhoto photo, string caption,
                                                     InlineKeyboardMarkup? keyboardMarkup, ITelegramBotClient botClient,
                                                     CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteMessage(userId, messageId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete message {MessageId} for user {UserId}", messageId, userId);
        }

        Message sentMessage = await botClient.SendPhoto(userId, new InputFileId(photo.TelegramFileId), caption,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
    }

    private async Task DeleteAndSendMediaGroupAsync(long userId, int messageId, List<ListingPhoto> photos,
                                                    string caption, InlineKeyboardMarkup? keyboardMarkup,
                                                    ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteMessage(userId, messageId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete message {MessageId} for user {UserId}", messageId, userId);
        }

        List<IAlbumInputMedia> mediaGroup = [];
        for (int i = 0; i < photos.Count; i++)
        {
            ListingPhoto photo = photos[i];
            if (i == 0)
                mediaGroup.Add(new InputMediaPhoto(photo.TelegramFileId)
                {
                    Caption = caption, ParseMode = ParseMode.Html
                });
            else
                mediaGroup.Add(new InputMediaPhoto(photo.TelegramFileId));
        }

        Message[] sentMessages =
            await botClient.SendMediaGroup(userId, mediaGroup, cancellationToken: cancellationToken);

        if (sentMessages.Length > 0 && keyboardMarkup != null && HasButtons(keyboardMarkup))
        {
            int firstMessageId = sentMessages[0].MessageId;
            try
            {
                await botClient.EditMessageReplyMarkup(userId, firstMessageId, keyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                _logger.LogInformation(
                    "Reply markup unchanged for media group first message {MessageId} for user {UserId}",
                    firstMessageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to edit reply markup for media group first message {MessageId} for user {UserId}",
                    firstMessageId, userId);
            }
        }
    }
}