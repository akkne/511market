namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ICallbackRouterService
{
    Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                             CancellationToken cancellationToken);
}