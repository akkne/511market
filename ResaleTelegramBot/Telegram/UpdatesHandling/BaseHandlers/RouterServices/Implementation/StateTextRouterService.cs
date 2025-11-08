namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Handlers.StateText.Abstract;

public class StateTextRouterService : IStateTextRouterService
{
    private readonly IEnumerable<IStateTextHandler> _handlers;

    public StateTextRouterService(IEnumerable<IStateTextHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task HandleStateAsync(Message message, ITelegramBotClient botClient,
                                       CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}