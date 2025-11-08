namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Shared;

public class StartCommandHandler : ICommandHandler
{
    public bool CanHandle(string command)
    {
        return command == CommandCodes.Start;
    }

    public async Task HandleCommandAsync(string args, Message message, ITelegramBotClient botClient,
                                         CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}