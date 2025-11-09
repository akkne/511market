namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface IRegularTextRouterService
{
    bool CanHandle(string message);

    Task HandleRegularTextAsync(Message message, ITelegramBotClient botClient,
                                CancellationToken cancellationToken);
}