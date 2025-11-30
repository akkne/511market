namespace ResaleTelegramBot.Telegram.Services.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Exceptions;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Models;

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

    public async Task<ListingViewResult> ShowShortListingAsync(long userId, int? mediaGroupMessageId,
                                                               int? buttonsMessageId, List<Listing> listings,
                                                               int startIndex, int totalListings, Guid? categoryId,
                                                               string searchText, ITelegramBotClient botClient,
                                                               CancellationToken cancellationToken)
    {
        if (listings.Count == 0)
        {
            _logger.LogWarning("Empty listings list");
            return new ListingViewResult
                { MediaGroupMessageId = mediaGroupMessageId, ButtonsMessageId = buttonsMessageId };
        }

        if (startIndex < 1 || startIndex > totalListings)
        {
            _logger.LogWarning("Invalid start index: {Index}, total: {Total}", startIndex, totalListings);
            return new ListingViewResult
                { MediaGroupMessageId = mediaGroupMessageId, ButtonsMessageId = buttonsMessageId };
        }

        string cardText = FormatListingsCard(listings, startIndex, totalListings);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnShortListingCard(listings, startIndex, totalListings, categoryId,
                searchText);

        List<ListingPhoto> photos = GetFirstPhotosFromListings(listings);
        const string buttonsText = "Выберите товар";

        if (photos.Count == 0)
        {
            int? messageIdToEdit = buttonsMessageId ?? mediaGroupMessageId;
            if (messageIdToEdit.HasValue)
            {
                try
                {
                    await botClient.EditMessageText(userId, messageIdToEdit.Value, cardText, ParseMode.Html,
                        keyboardMarkup, cancellationToken: cancellationToken);
                    return new ListingViewResult { ButtonsMessageId = messageIdToEdit.Value };
                }
                catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
                {
                    _logger.LogInformation("Message content unchanged for user {UserId}, message {MessageId}", userId,
                        messageIdToEdit.Value);
                }
            }
            else
            {
                Message sentMessage = await botClient.SendMessage(userId, cardText, ParseMode.Html,
                    replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                return new ListingViewResult { ButtonsMessageId = sentMessage.MessageId };
            }

            return new ListingViewResult { ButtonsMessageId = messageIdToEdit.Value };
        }

        if (photos.Count == 1)
        {
            int? messageIdToEdit = mediaGroupMessageId ?? buttonsMessageId;
            if (messageIdToEdit.HasValue)
                try
                {
                    string photoFileId = photos[0].TelegramFileId;
                    await botClient.EditMessageMedia(new ChatId(userId), messageIdToEdit.Value,
                        new InputMediaPhoto(photoFileId)
                        {
                            Caption = cardText, ParseMode = ParseMode.Html
                        }, keyboardMarkup, cancellationToken: cancellationToken);
                    return new ListingViewResult { MediaGroupMessageId = messageIdToEdit.Value };
                }
                catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
                {
                    _logger.LogInformation("Message content unchanged for user {UserId}, message {MessageId}", userId,
                        messageIdToEdit.Value);
                    return new ListingViewResult { MediaGroupMessageId = messageIdToEdit.Value };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to edit message media for user {UserId}, message {MessageId}. Deleting and resending.",
                        userId, messageIdToEdit.Value);
                    try
                    {
                        await botClient.DeleteMessage(userId, messageIdToEdit.Value, cancellationToken);
                    }
                    catch
                    {
                        // Ignore delete errors
                    }
                }

            Message sentPhoto = await botClient.SendPhoto(userId, photos[0].TelegramFileId, cardText,
                ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
            return new ListingViewResult { MediaGroupMessageId = sentPhoto.MessageId };
        }

        return await SendOrUpdateMediaGroupWithButtonsAsync(userId, mediaGroupMessageId, buttonsMessageId, photos,
            cardText, buttonsText, keyboardMarkup, botClient, cancellationToken);
    }

    public Task ShowLongListingAsync()
    {
        throw new NotImplementedException();
    }

    private static string FormatListingsCard(List<Listing> listings, int startIndex, int totalListings)
    {
        _ = totalListings; // Used for future formatting
        List<string> listingTexts = [];
        for (int i = 0; i < listings.Count && i < 4; i++)
        {
            Listing listing = listings[i];
            int listingNumber = startIndex + i;
            listingTexts.Add($"""
                              {listingNumber}. <b>{listing.Title}</b>
                              💰 {listing.Price} ₽
                              📂 {listing.Category.Name}
                              """);
        }

        return string.Join("\n\n", listingTexts);
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

    private async Task<ListingViewResult> SendOrUpdateMediaGroupWithButtonsAsync(long userId, int? mediaGroupMessageId,
        int? buttonsMessageId,
        List<ListingPhoto> photos,
        string caption, string buttonsText,
        InlineKeyboardMarkup keyboardMarkup,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        int? newMediaGroupMessageId = null;
        int? newButtonsMessageId = buttonsMessageId;

        if (mediaGroupMessageId.HasValue)
            try
            {
                await botClient.DeleteMessage(userId, mediaGroupMessageId.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete old media group message {MessageId} for user {UserId}",
                    mediaGroupMessageId.Value, userId);
            }

        List<IAlbumInputMedia> mediaGroup = [];
        foreach (ListingPhoto photo in photos) mediaGroup.Add(new InputMediaPhoto(photo.TelegramFileId));

        mediaGroup[0] = new InputMediaPhoto(photos[0].TelegramFileId)
        {
            Caption = caption, ParseMode = ParseMode.Html
        };

        Message[] sentMessages =
            await botClient.SendMediaGroup(userId, mediaGroup, cancellationToken: cancellationToken);
        if (sentMessages.Length > 0) newMediaGroupMessageId = sentMessages[0].MessageId;

        if (buttonsMessageId.HasValue)
        {
            try
            {
                await botClient.EditMessageText(userId, buttonsMessageId.Value, buttonsText, ParseMode.Html,
                    keyboardMarkup, cancellationToken: cancellationToken);
                newButtonsMessageId = buttonsMessageId.Value;
            }
            catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                _logger.LogInformation("Buttons message unchanged for user {UserId}, message {MessageId}", userId,
                    buttonsMessageId.Value);
                newButtonsMessageId = buttonsMessageId.Value;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Failed to edit buttons message {MessageId} for user {UserId}. Sending new message.",
                    buttonsMessageId.Value, userId);
                try
                {
                    await botClient.DeleteMessage(userId, buttonsMessageId.Value, cancellationToken);
                }
                catch
                {
                    // Ignore delete errors
                }

                Message sentButtonsMessage = await botClient.SendMessage(userId, buttonsText, ParseMode.Html,
                    replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                newButtonsMessageId = sentButtonsMessage.MessageId;
            }
        }
        else
        {
            Message sentButtonsMessage = await botClient.SendMessage(userId, buttonsText, ParseMode.Html,
                replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
            newButtonsMessageId = sentButtonsMessage.MessageId;
        }

        return new ListingViewResult
        {
            MediaGroupMessageId = newMediaGroupMessageId, ButtonsMessageId = newButtonsMessageId
        };
    }
}