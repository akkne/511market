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

    public async Task<ListingViewResult> ShowShortListingAsync(long userId, List<int>? mediaGroupMessageIdList,
                                                               int? buttonsMessageId, List<Listing> listings,
                                                               int startIndex, int totalListings, Guid? categoryId,
                                                               string searchText, ITelegramBotClient botClient,
                                                               CancellationToken cancellationToken)
    {
        if (listings.Count == 0)
        {
            _logger.LogWarning("Empty listings list");
            return new ListingViewResult
                { MediaGroupMessageId = mediaGroupMessageIdList, ButtonsMessageId = buttonsMessageId };
        }

        if (startIndex < 1 || startIndex > totalListings)
        {
            _logger.LogWarning("Invalid start index: {Index}, total: {Total}", startIndex, totalListings);
            return new ListingViewResult
                { MediaGroupMessageId = mediaGroupMessageIdList, ButtonsMessageId = buttonsMessageId };
        }

        string cardText = FormatListingsCard(listings, startIndex, totalListings);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnShortListingCard(listings, startIndex, totalListings, categoryId,
                searchText);

        List<ListingPhoto> photos = GetFirstPhotosFromListings(listings);
        const string buttonsText = "Выберите товар";

        if (photos.Count == 0)
        {
            if (buttonsMessageId.HasValue)
            {
                try
                {
                    await botClient.EditMessageText(userId, buttonsMessageId.Value, cardText, ParseMode.Html,
                        keyboardMarkup, cancellationToken: cancellationToken);
                    return new ListingViewResult { ButtonsMessageId = buttonsMessageId.Value };
                }
                catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
                {
                    _logger.LogInformation("Message content unchanged for user {UserId}, message {MessageId}", userId,
                        buttonsMessageId.Value);
                }
            }
            else
            {
                Message sentMessage = await botClient.SendMessage(userId, cardText, ParseMode.Html,
                    replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                return new ListingViewResult { ButtonsMessageId = sentMessage.MessageId };
            }

            return new ListingViewResult { ButtonsMessageId = buttonsMessageId.Value };
        }

        return await SendOrUpdateMediaGroupWithButtonsAsync(userId, mediaGroupMessageIdList, buttonsMessageId, photos,
            cardText, buttonsText, keyboardMarkup, botClient, cancellationToken);
    }

    public async Task ShowLongListingAsync(long userId, Listing listing, ITelegramBotClient botClient,
                                           CancellationToken cancellationToken)
    {
        string cardText = FormatLongListingCard(listing);
        List<ListingPhoto> photos = listing.Photos.OrderBy(p => p.Order).ToList();

        switch (photos.Count)
        {
            case 0:
                await botClient.SendMessage(userId, cardText, ParseMode.Html, cancellationToken: cancellationToken);
                return;
            case 1:
                await botClient.SendPhoto(userId, new InputFileId(photos[0].TelegramFileId), cardText,
                    ParseMode.Html, cancellationToken: cancellationToken);
                return;
        }

        List<InputMediaPhoto> mediaGroup = [];
        mediaGroup.AddRange(photos.Select(photo => new InputMediaPhoto(photo.TelegramFileId)));

        mediaGroup[0] = new InputMediaPhoto(photos[0].TelegramFileId)
        {
            Caption = cardText,
            ParseMode = ParseMode.Html
        };

        await botClient.SendMediaGroup(userId, mediaGroup, cancellationToken: cancellationToken);
    }

    private static string FormatListingsCard(List<Listing> listings, int startIndex, int totalListings)
    {
        _ = totalListings;
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

    private static string FormatLongListingCard(Listing listing)
    {
        string sellerInfo = listing.SellerProfile.UserProfile.TelegramData.Username != null
            ? $"@{listing.SellerProfile.UserProfile.TelegramData.Username}"
            : $"ID: {listing.SellerProfile.UserProfile.TelegramData?.Id ?? 0}";

        return $"""
                <b>{listing.Title}</b>

                💰 <b>Цена:</b> {listing.Price} ₽

                📂 <b>Категория:</b> {listing.Category.Name}

                👤 <b>Продавец:</b> {sellerInfo}

                📝 <b>Описание:</b>
                {listing.Description}

                📅 <b>Опубликовано:</b> {listing.CreatedAt:dd.MM.yyyy HH:mm}
                """;
    }

    private static List<ListingPhoto> GetFirstPhotosFromListings(List<Listing> listings)
    {
        List<ListingPhoto> photos = [];
        photos.AddRange(from listing in listings
                        where listing.Photos.Count != 0
                        select listing.Photos.OrderBy(p => p.Order).FirstOrDefault());

        return photos;
    }

    private async Task<ListingViewResult> SendOrUpdateMediaGroupWithButtonsAsync(
        long userId, List<int>? mediaGroupMessageIdList,
        int? buttonsMessageId,
        List<ListingPhoto> photos,
        string caption, string buttonsText,
        InlineKeyboardMarkup keyboardMarkup,
        ITelegramBotClient botClient,
        CancellationToken cancellationToken)
    {
        List<InputMediaPhoto> mediaGroup = [];
        mediaGroup.AddRange(photos.Select(photo => new InputMediaPhoto(photo.TelegramFileId)));

        mediaGroup[0] = new InputMediaPhoto(photos[0].TelegramFileId)
        {
            Caption = caption, ParseMode = ParseMode.Html
        };

        if (mediaGroupMessageIdList == null)
        {
            Message[] sentMessages =
                await botClient.SendMediaGroup(userId, mediaGroup, cancellationToken: cancellationToken);
            if (sentMessages.Length > 0)
                mediaGroupMessageIdList = sentMessages.Select(x => x.MessageId).ToList();
        }
        else
        {
            List<Message> messageSend = [];
            for (int i = 0; i < mediaGroupMessageIdList.Count; i++)
                try
                {
                    if (i < mediaGroup.Count)
                    {
                        InputMediaPhoto media = mediaGroup[i];
                        Message photoMessage = await botClient.EditMessageMedia(userId, mediaGroupMessageIdList[i],
                            media,
                            cancellationToken: cancellationToken);
                        messageSend.Add(photoMessage);
                    }
                    else
                    {
                        await botClient.DeleteMessage(userId, mediaGroupMessageIdList[i], cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to edit/delete message with id: {messageId} in chat: {chatId}",
                        mediaGroupMessageIdList[i], userId);
                }

            mediaGroupMessageIdList = messageSend.Select(x => x.MessageId).ToList();
        }

        if (buttonsMessageId.HasValue)
        {
            try
            {
                await botClient.EditMessageText(userId, buttonsMessageId.Value, buttonsText, ParseMode.Html,
                    keyboardMarkup, cancellationToken: cancellationToken);
            }
            catch (RequestException ex) when (ex.Message.Contains("message is not modified"))
            {
                _logger.LogInformation("Buttons message unchanged for user {UserId}, message {MessageId}", userId,
                    buttonsMessageId.Value);
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
                    _logger.LogInformation("Failed to edit message with id");
                }

                Message sentButtonsMessage = await botClient.SendMessage(userId, buttonsText, ParseMode.Html,
                    replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
                buttonsMessageId = sentButtonsMessage.MessageId;
            }
        }
        else
        {
            Message sentButtonsMessage = await botClient.SendMessage(userId, buttonsText, ParseMode.Html,
                replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
            buttonsMessageId = sentButtonsMessage.MessageId;
        }

        return new ListingViewResult
        {
            MediaGroupMessageId = mediaGroupMessageIdList, ButtonsMessageId = buttonsMessageId
        };
    }
}