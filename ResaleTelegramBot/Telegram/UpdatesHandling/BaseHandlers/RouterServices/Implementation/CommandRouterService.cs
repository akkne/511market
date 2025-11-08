namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public class CommandRouterService : ICommandRouterService
{
    public async Task HandleCommandAsync(string command, string args, Message message, ITelegramBotClient botClient,
                                         CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}