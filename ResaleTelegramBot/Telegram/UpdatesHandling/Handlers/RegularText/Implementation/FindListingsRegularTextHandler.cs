namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.RegularText.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Texts.Input;
using Texts.Output;

public class FindListingsRegularTextHandler : IRegularTextHandler
{
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ILogger<FindListingsRegularTextHandler> _logger;

    public FindListingsRegularTextHandler(ILogger<FindListingsRegularTextHandler> logger,
                                          ICallbackKeyboardGenerator callbackKeyboardGenerator)
    {
        _logger = logger;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
    }

    public bool CanHandle(string message)
    {
        return message == RegularTextStaticTexts.FindListings;
    }

    public async Task HandleRegularTextAsync(Message message, ITelegramBotClient botClient,
                                             CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            _logger.LogWarning("From is null");
            return;
        }

        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnSearchTypeSelection();

        await botClient.SendMessage(message.From.Id, ResponseMessageStaticTexts.OnSearchTypeSelection,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
    }
}