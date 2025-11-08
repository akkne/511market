namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ICommandRouterService
{
    Task HandleCommandAsync(string command, string args, Message message, ITelegramBotClient botClient,
                            CancellationToken cancellationToken);
}