namespace ResaleTelegramBot.Telegram.Services.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Texts.Output;

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

    public async Task ShowListingAsync(long userId, int messageId, Listing listing, int listingIndex,
                                       int totalListings, Guid? categoryId, string searchText,
                                       ITelegramBotClient botClient, CancellationToken cancellationToken)
    {
        if (listingIndex < 1 || listingIndex > totalListings)
        {
            _logger.LogWarning("Invalid listing index: {Index}, total: {Total}", listingIndex, totalListings);
            return;
        }

        string cardText = FormatListingCard(listing, listingIndex, totalListings);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnShortListingCard(listing, listingIndex, totalListings, categoryId,
                searchText);

        if (listing.Photos.Count > 0)
        {
            string photoFileId = listing.Photos[0].TelegramFileId;
            await botClient.EditMessageMedia(new ChatId(userId), messageId,
                new InputMediaPhoto(photoFileId)
                {
                    Caption = cardText, ParseMode = ParseMode.Html
                }, keyboardMarkup, cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.EditMessageText(userId, messageId, cardText, ParseMode.Html,
                keyboardMarkup, cancellationToken: cancellationToken);
        }
    }

    private static string FormatListingCard(Listing listing, int listingNumber, int totalListings)
    {
        return $"""
                {ResponseMessageStaticTexts.OnListingCard(listingNumber, totalListings)}
                <b>{listing.Title}</b>

                💰 {listing.Price} ₽
                📂 {listing.Category.Name}
                """;
    }
}