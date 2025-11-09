namespace ResaleTelegramBot.Telegram.Scenes.Contexts.Abstract;

public abstract class BaseSceneContext
{
    public long UserId { get; set; }
    public string SceneName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}