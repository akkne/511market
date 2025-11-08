namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.StateText.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface IStateTextHandler
{
    bool CanHandle(Message message);

    Task HandleStateTextAsync(Message message, ITelegramBotClient botClient,
                              CancellationToken cancellationToken);
}