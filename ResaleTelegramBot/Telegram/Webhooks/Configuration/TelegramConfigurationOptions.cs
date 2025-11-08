namespace ResaleTelegramBot.Telegram.Webhooks.Configuration;

public class TelegramConfigurationOptions
{
    public const string SectionName = "TelegramConfiguration";

    public required string TelegramBotToken { get; set; }
    public required string MiniAppUrl { get; set; }
    public string? WebhookSecretToken { get; set; }
}