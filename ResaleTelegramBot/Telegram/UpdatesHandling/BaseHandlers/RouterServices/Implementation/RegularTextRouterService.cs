namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Handlers.RegularText.Abstract;

public class RegularTextRouterService : IRegularTextRouterService
{
    private readonly IEnumerable<IRegularTextHandler> _handlers;

    public RegularTextRouterService(IEnumerable<IRegularTextHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task HandleStateAsync(Message message, ITelegramBotClient botClient,
                                       CancellationToken cancellationToken)
    {
        IRegularTextHandler? handler = _handlers.FirstOrDefault(h => h.CanHandle(message.Text ?? string.Empty));
        if (handler == null) throw new InvalidOperationException("Handler not found");

        await handler.HandleRegularTextAsync(message, botClient, cancellationToken);
    }
}