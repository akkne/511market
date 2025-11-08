namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers;

using global::Telegram.Bot;
using global::Telegram.Bot.Polling;
using global::Telegram.Bot.Types;

public class MessageUpdatesHandler : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
                                       CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}