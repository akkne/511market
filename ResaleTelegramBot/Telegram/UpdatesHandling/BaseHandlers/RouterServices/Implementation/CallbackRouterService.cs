namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public class CallbackRouterService : ICallbackRouterService
{
    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                          CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}