namespace ResaleTelegramBot.Telegram.Webhooks.Services.Abstract;

using global::Telegram.Bot.Types;

public interface IWebhookConfigurationService
{
    Task<bool> SetWebhookAsync(string webhookUrl, string[]? allowedUpdates = null,
                               CancellationToken cancellationToken = default);

    Task DeleteWebhookAsync(CancellationToken cancellationToken = default);

    Task<WebhookInfo?> GetWebhookInfoAsync(CancellationToken cancellationToken = default);

    bool ValidateWebhookUrl(string webhookUrl);
}