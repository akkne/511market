namespace ResaleTelegramBot.Telegram.Webhooks.Services.Hosted;

using Abstract;
using Microsoft.Extensions.Options;
using Options;

public class WebhookReceiverService : IHostedService
{
    private readonly ILogger<WebhookReceiverService> _logger;
    private readonly WebhookConfigurationOptions _options;
    private readonly IWebhookConfigurationService _webhookConfigurationService;

    public WebhookReceiverService(
        IWebhookConfigurationService webhookConfigurationService,
        ILogger<WebhookReceiverService> logger,
        IOptions<WebhookConfigurationOptions> options)
    {
        _webhookConfigurationService = webhookConfigurationService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            string webhookUrl = _options.WebhookUrl;

            if (string.IsNullOrEmpty(webhookUrl))
            {
                _logger.LogWarning("WebhookUrl not configured, skipping webhook registration");
                return;
            }

            _logger.LogInformation("Starting webhook registration");

            bool success =
                await _webhookConfigurationService.SetWebhookAsync(webhookUrl, cancellationToken: cancellationToken);

            if (success)
                _logger.LogInformation("Webhook registered successfully at: {WebhookUrl}", webhookUrl);
            else
                _logger.LogError("Failed to register webhook at: {WebhookUrl}", webhookUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during webhook registration");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Stopping webhook receiver service");
            await _webhookConfigurationService.DeleteWebhookAsync(cancellationToken);
            _logger.LogInformation("Webhook deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook during shutdown");
        }
    }
}