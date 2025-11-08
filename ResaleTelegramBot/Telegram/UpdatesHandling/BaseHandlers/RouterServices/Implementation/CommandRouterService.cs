namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Handlers.Command.Abstract;

public class CommandRouterService : ICommandRouterService
{
    private readonly IEnumerable<ICommandHandler> _handlers;

    public CommandRouterService(IEnumerable<ICommandHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task HandleCommandAsync(string command, string args, Message message, ITelegramBotClient botClient,
                                         CancellationToken cancellationToken)
    {
        ICommandHandler? handler = _handlers.FirstOrDefault(h => h.CanHandle(command));
        if (handler == null) throw new InvalidOperationException("Handler not found");

        await handler.HandleCommandAsync(args, message, botClient, cancellationToken);
    }
}