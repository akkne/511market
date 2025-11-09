namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Implementation;

using Abstract;
using Core.Shared.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using Services.Abstract;
using Services.Contracts;
using Shared.Enums;
using Texts.Output;
using User = Core.Models.User;

public class StartCommandHandler : ICommandHandler
{
    private readonly IAuthService _authService;
    private readonly ILogger<StartCommandHandler> _logger;
    private readonly IReplyKeyboardGenerator _replyKeyboardGenerator;

    public StartCommandHandler(ILogger<StartCommandHandler> logger, IReplyKeyboardGenerator replyKeyboardGenerator,
                               IAuthService authService)
    {
        _logger = logger;
        _replyKeyboardGenerator = replyKeyboardGenerator;
        _authService = authService;
    }

    public bool CanHandle(string command)
    {
        return command == CommandCodes.Start;
    }

    public async Task HandleCommandAsync(string args, Message message, ITelegramBotClient botClient,
                                         CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            _logger.LogWarning("From is null");
            return;
        }

        User? existingUser = await _authService.GetByTelegramIdAsync(message.From.Id, cancellationToken);
        if (existingUser == null)
        {
            _logger.LogInformation("User with id: {userId} not found", message.From.Id);
            User? created = await _authService.RegisterUserAsync(
                UserRegistrationContract.Create(TelegramData.Create(message.From.Id, message.From.Username)),
                cancellationToken);
            if (created == null)
            {
                _logger.LogWarning("Failed to register user with id: {userId}", message.From.Id);
                return;
            }

            _logger.LogInformation("User with id: {userId} successfully registered", created.Id);
        }

        ReplyMarkup keyboard = _replyKeyboardGenerator.GenerateKeyboardMarkup(KeyboardGenerationCodes.MainMenu);
        string responseText = ResponseMessageStaticTexts.OnStart;

        await botClient.SendMessage(message.From.Id, responseText, ParseMode.Html, replyMarkup: keyboard,
            cancellationToken: cancellationToken);
    }
}