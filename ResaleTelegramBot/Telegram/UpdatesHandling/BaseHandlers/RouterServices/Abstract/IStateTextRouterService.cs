namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface IStateTextRouterService
{
    Task HandleStateAsync(Message message, ITelegramBotClient botClient,
                          CancellationToken cancellationToken);
}