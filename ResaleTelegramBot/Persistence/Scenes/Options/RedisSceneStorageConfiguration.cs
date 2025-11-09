namespace ResaleTelegramBot.Persistence.Scenes.Options;

public class RedisSceneStorageConfiguration
{
    public const string SectionName = "RedisSceneStorageConfiguration";

    public string ConnectionString { get; set; } = null!;
    public int TtlSeconds { get; set; }
    public bool AbortOnConnectFail { get; set; }
}