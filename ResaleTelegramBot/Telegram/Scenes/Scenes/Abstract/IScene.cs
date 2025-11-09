namespace ResaleTelegramBot.Telegram.Scenes.Scenes.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface IScene
{
    string SceneName { get; }

    Task EnterAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken);

    Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken cancellationToken);

    Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                             CancellationToken cancellationToken);

    Task ExitAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken);
}