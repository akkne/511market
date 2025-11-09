namespace ResaleTelegramBot.Telegram.Scenes.Gateway.Abstract;

using global::Telegram.Bot;
using global::Telegram.Bot.Types;

public interface ISceneGatewayService
{
    Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot, CancellationToken cancellationToken);

    Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                             CancellationToken cancellationToken);

    Task EnterSceneAsync(long userId, string sceneName, ITelegramBotClient bot, CancellationToken cancellationToken);
    Task ExitCurrentSceneAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken);
}