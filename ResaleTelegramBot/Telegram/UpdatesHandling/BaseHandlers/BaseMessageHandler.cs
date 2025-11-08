namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers;

using global::Telegram.Bot;
using global::Telegram.Bot.Exceptions;
using global::Telegram.Bot.Polling;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using RouterServices.Abstract;

public class BaseUpdatesHandler : IUpdateHandler
{
    private readonly ICommandRouterService _commandRouterService;
    private readonly ILogger<BaseUpdatesHandler> _logger;

    public BaseUpdatesHandler(
        ILogger<BaseUpdatesHandler> logger, ICommandRouterService commandRouterService)
    {
        _logger = logger;
        _commandRouterService = commandRouterService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
                                        CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        Message message = update.Message;

        if (update.Message.Type == MessageType.Text)
        {
            if (message.From == null) return;

            await HandleTextMessageAsync(botClient, message, cancellationToken);
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
                                       CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);

        if (exception is RequestException) await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message,
                                              CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received message: {text} from user with username: {username} with id: {id}",
            message.Text, message.From!.Username, message.From.Id);

        if (message.Text!.StartsWith('/'))
        {
            await OnCommandAsync(botClient, message, cancellationToken);
        }
    }

    private async Task OnCommandAsync(ITelegramBotClient botClient, Message message,
                                      CancellationToken cancellationToken)
    {
        (string command, string args) = GetCommandArgumentsObjectAsync(message.Text!);

        _logger.LogInformation("Received command: {command} {args}", command, args);

        try
        {
            await _commandRouterService.HandleCommandAsync(command, args, message, botClient, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception.Message);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }
    }

    private static (string, string) GetCommandArgumentsObjectAsync(string messageText)
    {
        int spaceIndex = messageText.IndexOf(' ');
        if (spaceIndex < 0) spaceIndex = messageText.Length;

        string command = messageText[..spaceIndex].ToLower();
        string args = messageText[spaceIndex..].TrimStart();

        return (command, args);
    }
}