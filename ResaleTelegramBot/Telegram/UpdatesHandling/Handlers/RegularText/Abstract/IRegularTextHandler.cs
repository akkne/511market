namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.RegularText.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface IRegularTextHandler
{
    bool CanHandle(string message);

    Task HandleRegularTextAsync(Message message, ITelegramBotClient botClient,
                                CancellationToken cancellationToken);
}