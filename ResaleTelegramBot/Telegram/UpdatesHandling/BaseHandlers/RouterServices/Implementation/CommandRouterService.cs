namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Handlers.Command.Abstract;

public class CommandRouterService : ICommandRouterService
{
    private readonly IEnumerable<ICommandHandler> _commandHandlers;

    public CommandRouterService(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandHandlers = commandHandlers;
    }

    public async Task HandleCommandAsync(string command, string args, Message message, ITelegramBotClient botClient,
                                         CancellationToken cancellationToken)
    {
        ICommandHandler? handler = _commandHandlers.FirstOrDefault(h => h.CanHandle(command));
        if (handler == null) throw new InvalidOperationException("Handler not found");

        await handler.HandleCommandAsync(args, message, botClient, cancellationToken);
    }
}