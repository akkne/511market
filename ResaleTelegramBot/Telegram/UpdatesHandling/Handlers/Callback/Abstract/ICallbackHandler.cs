namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ICallbackHandler
{
    bool CanHandle(CallbackQuery callbackQuery);

    Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                             CancellationToken cancellationToken);
}