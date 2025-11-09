namespace ResaleTelegramBot.Telegram.Scenes.Gateway.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Managers.Abstract;

public class SceneGatewayService : ISceneGatewayService
{
    private readonly ILogger<SceneGatewayService> _logger;
    private readonly ISceneManager _sceneManager;

    public SceneGatewayService(ISceneManager sceneManager, ILogger<SceneGatewayService> logger)
    {
        _sceneManager = sceneManager;
        _logger = logger;
    }

    public async Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot,
                                         CancellationToken cancellationToken)
    {
        if (await _sceneManager.HasActiveSceneAsync(userId, cancellationToken))
        {
            await _sceneManager.HandleMessageAsync(userId, message, bot, cancellationToken);
            return;
        }

        _logger.LogWarning("No active scene for user {UserId}", userId);
    }

    public async Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                                          CancellationToken cancellationToken)
    {
        if (await _sceneManager.HasActiveSceneAsync(userId, cancellationToken))
        {
            await _sceneManager.HandleCallbackAsync(userId, callback, bot, cancellationToken);
            return;
        }

        _logger.LogWarning("No active scene for user {UserId}", userId);
    }

    public async Task EnterSceneAsync(long userId, string sceneName, ITelegramBotClient bot,
                                      CancellationToken cancellationToken)
    {
        await _sceneManager.EnterSceneAsync(userId, sceneName, bot, cancellationToken);
    }

    public async Task ExitCurrentSceneAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        await _sceneManager.ExitCurrentSceneAsync(userId, bot, cancellationToken);
    }
}