namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public class TextRouterService : ITextRouterService
{
    public async Task HandleStateAsync(Message message, ITelegramBotClient botClient,
                                       CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}