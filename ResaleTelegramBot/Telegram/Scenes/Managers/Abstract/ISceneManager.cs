namespace ResaleTelegramBot.Telegram.Scenes.Managers.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ISceneManager
{
    Task EnterSceneAsync(long userId, string sceneName, ITelegramBotClient bot, CancellationToken cancellationToken);
    Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken cancellationToken);

    Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                             CancellationToken cancellationToken);

    Task ExitCurrentSceneAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken);
    Task<bool> HasActiveSceneAsync(long userId, CancellationToken cancellationToken);
}