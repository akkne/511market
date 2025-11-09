namespace ResaleTelegramBot.Telegram.Scenes.Managers.Implementation;

using System.Collections.Concurrent;
using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Persistence.Scenes.Abstract;
using Scenes.Abstract;
using Shared;

public class SceneManager : ISceneManager
{
    private readonly ILogger<SceneManager> _logger;
    private readonly ConcurrentDictionary<string, IScene> _scenes = new();
    private readonly ISceneStorage _storage;

    public SceneManager(ISceneStorage storage, ILogger<SceneManager> logger, IEnumerable<IScene> scenes)
    {
        _storage = storage;
        _logger = logger;

        RegisterScene(scenes);
    }

    public async Task EnterSceneAsync(long userId, string sceneName, ITelegramBotClient bot,
                                      CancellationToken cancellationToken)
    {
        if (!SceneNames.All.Contains(sceneName))
        {
            _logger.LogWarning("Scene '{SceneName}' doesn't exist", sceneName);
            return;
        }

        if (!_scenes.TryGetValue(sceneName, out IScene? scene))
        {
            _logger.LogWarning("Scene '{SceneName}' not found", sceneName);
            return;
        }

        string? currentSceneName = await _storage.GetActiveSceneNameAsync(userId, cancellationToken);
        if (currentSceneName != null && _scenes.TryGetValue(currentSceneName, out IScene? currentScene))
            await currentScene.ExitAsync(userId, bot, cancellationToken);

        await _storage.SetActiveSceneAsync(userId, sceneName, cancellationToken);
        await scene.EnterAsync(userId, bot, cancellationToken);

        _logger.LogInformation("User {UserId} entered scene '{SceneName}'", userId, sceneName);
    }

    public async Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot,
                                         CancellationToken cancellationToken)
    {
        string? sceneName = await _storage.GetActiveSceneNameAsync(userId, cancellationToken);
        if (sceneName == null)
        {
            _logger.LogWarning("No active scene for user {UserId}", userId);
            return;
        }

        if (_scenes.TryGetValue(sceneName, out IScene? scene))
            await scene.HandleMessageAsync(userId, message, bot, cancellationToken);
    }

    public async Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                                          CancellationToken cancellationToken)
    {
        string? sceneName = await _storage.GetActiveSceneNameAsync(userId, cancellationToken);
        if (sceneName == null)
        {
            _logger.LogWarning("No active scene for user {UserId}", userId);
            return;
        }

        if (_scenes.TryGetValue(sceneName, out IScene? scene))
            await scene.HandleCallbackAsync(userId, callback, bot, cancellationToken);
    }

    public async Task ExitCurrentSceneAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        string? sceneName = await _storage.GetActiveSceneNameAsync(userId, cancellationToken);
        if (sceneName == null) return;

        if (_scenes.TryGetValue(sceneName, out IScene? scene)) await scene.ExitAsync(userId, bot, cancellationToken);

        await _storage.RemoveSceneContextAsync(userId, sceneName, cancellationToken);
        await _storage.SetActiveSceneAsync(userId, string.Empty, cancellationToken);
    }

    public async Task<bool> HasActiveSceneAsync(long userId, CancellationToken cancellationToken)
    {
        string? sceneName = await _storage.GetActiveSceneNameAsync(userId, cancellationToken);
        return !string.IsNullOrEmpty(sceneName);
    }


    private void RegisterScene(IEnumerable<IScene> scenes)
    {
        foreach (IScene scene in scenes)
        {
            _scenes[scene.SceneName] = scene;
            _logger.LogInformation("Scene '{SceneName}' registered", scene.SceneName);
        }
    }
}