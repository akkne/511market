namespace ResaleTelegramBot.Telegram.Webhooks.Services.Options;

public class WebhookConfigurationOptions
{
    public const string SectionName = "WebhookConfiguration";

    public required string WebhookUrl { get; set; }
}