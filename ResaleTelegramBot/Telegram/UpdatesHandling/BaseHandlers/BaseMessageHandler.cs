namespace ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers;

using global::Telegram.Bot;
using global::Telegram.Bot.Exceptions;
using global::Telegram.Bot.Polling;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using RouterServices.Abstract;

public class BaseUpdatesHandler : IUpdateHandler
{
    private readonly ICallbackRouterService _callbackRouterService;
    private readonly ICommandRouterService _commandRouterService;
    private readonly ILogger<BaseUpdatesHandler> _logger;
    private readonly IRegularTextRouterService _regularTextRouterService;

    public BaseUpdatesHandler(
        ILogger<BaseUpdatesHandler> logger, ICommandRouterService commandRouterService,
        ICallbackRouterService callbackRouterService, IRegularTextRouterService regularTextRouterService)
    {
        _logger = logger;
        _commandRouterService = commandRouterService;
        _callbackRouterService = callbackRouterService;
        _regularTextRouterService = regularTextRouterService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
                                        CancellationToken cancellationToken)
    {
        if (update.Message == null) return;

        Message message = update.Message;

        if (message.Type == MessageType.Text && message.Text != null)
        {
            if (message.From == null)
                return;

            _logger.LogInformation(
                "Received message: {text} from user with username: {username} with id: {id}",
                message.Text, message.From!.Username, message.From.Id);

            if (message.Text!.StartsWith('/')) await HandleCommandAsync(botClient, message, cancellationToken);

            if (_regularTextRouterService.CanHandle(message.Text))
                await HandleRegularTextAsync(botClient, message, cancellationToken);
        }

        if (update.CallbackQuery != null) await HandleCallbackAsync(botClient, update.CallbackQuery, cancellationToken);
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
                                       CancellationToken cancellationToken)
    {
        _logger.LogInformation("HandleError: {Exception}", exception);

        if (exception is RequestException) await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
    }

    private async Task HandleCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery,
                                           CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Received callback: {text} from user with username: {username} with id: {id}",
            callbackQuery.Data, callbackQuery.From.Username, callbackQuery.From.Id);

        try
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
            await _callbackRouterService.HandleCallbackAsync(callbackQuery, botClient, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }
    }

    private async Task HandleCommandAsync(ITelegramBotClient botClient, Message message,
                                          CancellationToken cancellationToken)
    {
        (string command, string args) = GetCommandArgumentsObjectAsync(message.Text!);

        _logger.LogInformation("Received command: {command} {args}", command, args);

        try
        {
            await _commandRouterService.HandleCommandAsync(command, args, message, botClient, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception.Message);
        }
    }

    private async Task HandleRegularTextAsync(ITelegramBotClient botClient, Message message,
                                              CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received regular text: {text}", message.Text!);

        try
        {
            await _regularTextRouterService.HandleRegularTextAsync(message, botClient, cancellationToken);
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