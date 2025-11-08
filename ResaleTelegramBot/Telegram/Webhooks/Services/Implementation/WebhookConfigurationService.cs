namespace ResaleTelegramBot.Telegram.Webhooks.Services.Implementation;

using Abstract;
using Configuration;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;

public class WebhookConfigurationService : IWebhookConfigurationService
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<WebhookConfigurationService> _logger;
    private readonly TelegramConfigurationOptions _telegramOptions;

    public WebhookConfigurationService(
        ITelegramBotClient botClient,
        ILogger<WebhookConfigurationService> logger,
        IOptions<TelegramConfigurationOptions> telegramOptions)
    {
        _botClient = botClient;
        _logger = logger;
        _telegramOptions = telegramOptions.Value;
    }

    public async Task<bool> SetWebhookAsync(string webhookUrl, string[]? allowedUpdates = null,
                                            CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ValidateWebhookUrl(webhookUrl))
            {
                _logger.LogError("Invalid webhook URL: {WebhookUrl}", webhookUrl);
                return false;
            }

            _logger.LogInformation("Setting webhook to: {WebhookUrl}", webhookUrl);

            await _botClient.SetWebhook(
                webhookUrl,
                allowedUpdates: allowedUpdates?.Select(u => Enum.Parse<UpdateType>(u, true)).ToArray() ??
                                [UpdateType.Message, UpdateType.CallbackQuery],
                secretToken: _telegramOptions.WebhookSecretToken,
                cancellationToken: cancellationToken);

            User bot = await _botClient.GetMe(cancellationToken);

            _logger.LogInformation("Webhook for bot: @{BotUsername} set successfully to: {WebhookUrl}", bot.Username,
                webhookUrl);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting webhook to: {WebhookUrl}", webhookUrl);
            return false;
        }
    }

    public async Task DeleteWebhookAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Deleting webhook");

            await _botClient.DeleteWebhook(cancellationToken: cancellationToken);

            _logger.LogInformation("Webhook deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook");
        }
    }

    public async Task<WebhookInfo?> GetWebhookInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            WebhookInfo webhookInfo = await _botClient.GetWebhookInfo(cancellationToken);

            _logger.LogInformation(
                "Webhook info retrieved. URL: {Url}, Has custom certificate: {HasCustomCertificate}, Pending update count: {PendingUpdateCount}",
                webhookInfo.Url, webhookInfo.HasCustomCertificate, webhookInfo.PendingUpdateCount);

            return webhookInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook info");
            return null;
        }
    }

    public bool ValidateWebhookUrl(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl)) return false;

        try
        {
            Uri uri = new(webhookUrl);

            if (uri.Scheme == "https")
                return Uri.IsWellFormedUriString(webhookUrl, UriKind.Absolute);

            _logger.LogWarning("Webhook URL must use HTTPS scheme: {WebhookUrl}", webhookUrl);
            return false;
        }
        catch (UriFormatException)
        {
            _logger.LogWarning("Invalid webhook URL format: {WebhookUrl}", webhookUrl);
            return false;
        }
    }
}