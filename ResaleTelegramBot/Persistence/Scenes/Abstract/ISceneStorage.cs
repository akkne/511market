namespace ResaleTelegramBot.Persistence.Scenes.Abstract;

using Telegram.Scenes.Contexts.Abstract;

public interface ISceneStorage
{
    Task<TContext?> GetSceneContextAsync<TContext>(long userId, string sceneName, CancellationToken cancellationToken)
        where TContext : BaseSceneContext;

    Task<TContext> GetOrCreateSceneContextAsync<TContext>(
        long userId,
        string sceneName,
        Func<TContext> factory,
        CancellationToken ct)
        where TContext : BaseSceneContext;

    Task SaveSceneContextAsync<TContext>(long userId, string sceneName, TContext context,
                                         CancellationToken cancellationToken)
        where TContext : BaseSceneContext;

    Task RemoveSceneContextAsync(long userId, string sceneName, CancellationToken cancellationToken);
    Task RemoveAllScenesAsync(long userId, CancellationToken cancellationToken);

    Task<string?> GetActiveSceneNameAsync(long userId, CancellationToken cancellationToken);
    Task SetActiveSceneAsync(long userId, string sceneName, CancellationToken cancellationToken);
}