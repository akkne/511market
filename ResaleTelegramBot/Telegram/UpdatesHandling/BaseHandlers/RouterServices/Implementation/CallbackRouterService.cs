namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Handlers.Callback.Abstract;

public class CallbackRouterService : ICallbackRouterService
{
    private readonly IEnumerable<ICallbackHandler> _handlers;

    public CallbackRouterService(IEnumerable<ICallbackHandler> handlers)
    {
        _handlers = handlers;
    }

    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                          CancellationToken cancellationToken)
    {
        ICallbackHandler? handler = _handlers.FirstOrDefault(h => h.CanHandle(callbackQuery));
        if (handler == null) throw new InvalidOperationException("Handler not found");

        await handler.HandleCallbackAsync(callbackQuery, botClient, cancellationToken);
    }
}