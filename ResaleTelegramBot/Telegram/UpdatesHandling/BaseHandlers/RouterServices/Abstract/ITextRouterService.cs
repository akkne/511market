namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ITextRouterService
{
    Task HandleStateAsync(Message message, ITelegramBotClient botClient,
                          CancellationToken cancellationToken);
}