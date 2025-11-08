namespace ResaleTelegramBot.Telegram.Webhooks.Endpoints;

using Configuration;
using global::Telegram.Bot;
using global::Telegram.Bot.Polling;
using global::Telegram.Bot.Types;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

public static class WebhookEndpoints
{
    private const string TelegramSecretTokenHeader = "X-Telegram-Bot-Api-Secret-Token";

    public static WebApplication MapWebhookEndpoints(this WebApplication app)
    {
        app.MapPost("/api/webhook", HandleWebhookAsync);

        return app;
    }

    private static async Task<Results<Ok, UnauthorizedHttpResult, InternalServerError<int>>> HandleWebhookAsync(
        Update update, IUpdateHandler messageHandler,
        ITelegramBotClient botClient, ILogger<Program> logger, HttpRequest request,
        IOptions<TelegramConfigurationOptions> configurationOptions)
    {
        try
        {
            string? secretToken = request.Headers[TelegramSecretTokenHeader].FirstOrDefault();

            if (string.IsNullOrEmpty(secretToken))
            {
                logger.LogWarning("No secret token provided in webhook request");
                return TypedResults.Unauthorized();
            }

            if (secretToken != configurationOptions.Value.WebhookSecretToken)
            {
                logger.LogWarning("Invalid secret token provided in webhook request");
                return TypedResults.Unauthorized();
            }

            logger.LogInformation("Received update: {UpdateId}", update.Id);
            await messageHandler.HandleUpdateAsync(botClient, update, CancellationToken.None);
            return TypedResults.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update {UpdateId}", update.Id);
            return TypedResults.InternalServerError(StatusCodes.Status500InternalServerError);
        }
    }
}