namespace ResaleTelegramBot.Persistence.Scenes.Implementation;

using System.Text.Json;
using Abstract;
using Microsoft.Extensions.Options;
using Options;
using StackExchange.Redis;
using Telegram.Scenes.Contexts.Abstract;

public class RedisSceneStorage : ISceneStorage
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<RedisSceneStorage> _logger;
    private readonly IDatabase _redisDatabase;
    private readonly RedisSceneStorageConfiguration _redisSceneStorageConfiguration;

    public RedisSceneStorage(
        ILogger<RedisSceneStorage> logger,
        IConnectionMultiplexer redis, IOptions<RedisSceneStorageConfiguration> redisSceneStorageConfiguration)
    {
        _logger = logger;
        _redisSceneStorageConfiguration = redisSceneStorageConfiguration.Value;
        _redisDatabase = redis.GetDatabase(_redisSceneStorageConfiguration.DatabaseIndex);
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public async Task<TContext?> GetSceneContextAsync<TContext>(long userId, string sceneName,
                                                                CancellationToken cancellationToken)
        where TContext : BaseSceneContext
    {
        RedisKey key = GetContextKey(userId, sceneName);
        RedisValue value = await _redisDatabase.StringGetAsync(key).ConfigureAwait(false);
        if (value.IsNullOrEmpty) return null;

        try
        {
            TContext? context = JsonSerializer.Deserialize<TContext>(value!, _jsonSerializerOptions);
            return context;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to deserialize scene context for user {UserId}, scene {SceneName}",
                userId, sceneName);
            return null;
        }
    }

    public async Task<TContext> GetOrCreateSceneContextAsync<TContext>(
        long userId,
        string sceneName,
        Func<TContext> factory,
        CancellationToken cancellationToken)
        where TContext : BaseSceneContext
    {
        TContext? existing = await GetSceneContextAsync<TContext>(userId, sceneName, cancellationToken)
           .ConfigureAwait(false);
        if (existing != null) return existing;

        TContext context = factory();
        context.UserId = userId;
        context.SceneName = sceneName;
        context.CreatedAt = DateTime.UtcNow;
        context.UpdatedAt = DateTime.UtcNow;

        await SaveSceneContextAsync(userId, sceneName, context, cancellationToken).ConfigureAwait(false);
        return context;
    }

    public async Task SaveSceneContextAsync<TContext>(long userId, string sceneName, TContext context,
                                                      CancellationToken cancellationToken)
        where TContext : BaseSceneContext
    {
        context.UpdatedAt = DateTime.UtcNow;

        string json = JsonSerializer.Serialize(context, _jsonSerializerOptions);
        RedisKey key = GetContextKey(userId, sceneName);
        RedisKey indexKey = GetIndexKey(userId);

        await _redisDatabase.StringSetAsync(key, json);
        await _redisDatabase.KeyExpireAsync(key, TimeSpan.FromSeconds(_redisSceneStorageConfiguration.TtlSeconds));

        bool ok = await _redisDatabase.SetAddAsync(indexKey, sceneName);
        await _redisDatabase.KeyExpireAsync(indexKey, TimeSpan.FromSeconds(_redisSceneStorageConfiguration.TtlSeconds));

        if (!ok)
            _logger.LogWarning("Redis transaction failed when saving context for user {UserId}, scene {SceneName}",
                userId, sceneName);
    }

    public async Task RemoveSceneContextAsync(long userId, string sceneName, CancellationToken cancellationToken)
    {
        try
        {
            RedisKey key = GetContextKey(userId, sceneName);
            RedisKey indexKey = GetIndexKey(userId);

            _logger.LogInformation("Removing scene context for user {UserId}, scene {SceneName}", userId, sceneName);

            bool keyDeleted = await _redisDatabase.KeyDeleteAsync(key).ConfigureAwait(false);
            bool isRemoved = await _redisDatabase.SetRemoveAsync(indexKey, sceneName).ConfigureAwait(false);

            if (!isRemoved || !keyDeleted) _logger.LogWarning("Failed to remove user's scene");

            _logger.LogInformation(
                "Removed scene context for user {UserId}, scene {SceneName}. Key deleted: {KeyDeleted}",
                userId, sceneName, keyDeleted);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                "Failed to remove scene context for user {UserId}, scene {SceneName}",
                userId, sceneName);
            throw;
        }
    }

    public async Task RemoveAllScenesAsync(long userId, CancellationToken cancellationToken)
    {
        RedisKey indexKey = GetIndexKey(userId);
        RedisValue[] members = await _redisDatabase.SetMembersAsync(indexKey).ConfigureAwait(false);
        if (members.Length == 0)
        {
            await _redisDatabase.KeyDeleteAsync(indexKey).ConfigureAwait(false);
            return;
        }

        RedisKey[] keys = members
                         .Select(m => GetContextKey(userId, m.ToString()))
                         .Concat([indexKey])
                         .ToArray();

        await _redisDatabase.KeyDeleteAsync(keys).ConfigureAwait(false);
        await _redisDatabase.KeyDeleteAsync(GetActiveKey(userId)).ConfigureAwait(false);
    }

    public async Task<string?> GetActiveSceneNameAsync(long userId, CancellationToken cancellationToken)
    {
        RedisValue value = await _redisDatabase.StringGetAsync(GetActiveKey(userId)).ConfigureAwait(false);
        return value.IsNullOrEmpty ? null : (string?)value!;
    }

    public async Task SetActiveSceneAsync(long userId, string sceneName, CancellationToken cancellationToken)
    {
        RedisKey activeKey = GetActiveKey(userId);

        if (string.IsNullOrEmpty(sceneName))
        {
            await _redisDatabase.KeyDeleteAsync(activeKey).ConfigureAwait(false);
            _logger.LogInformation("Removed active scene for user {UserId}", userId);
            return;
        }

        await _redisDatabase.StringSetAsync(activeKey, sceneName,
            TimeSpan.FromSeconds(_redisSceneStorageConfiguration.TtlSeconds)).ConfigureAwait(false);
        _logger.LogInformation("Set active scene for user {UserId} to {SceneName}", userId, sceneName);
    }

    private static RedisKey GetContextKey(long userId, string sceneName)
    {
        return $"scene:{userId}:ctx:{sceneName}";
    }

    private static RedisKey GetActiveKey(long userId)
    {
        return $"scene:{userId}:active";
    }

    private static RedisKey GetIndexKey(long userId)
    {
        return $"scene:{userId}:index";
    }
}