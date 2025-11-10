namespace ResaleTelegramBot.Telegram.Scenes.Scenes.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Contexts.Implementation;
using Contexts.SceneSteps;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using Persistence.Scenes.Abstract;
using Services.Abstract;
using Services.Contracts.Listing;
using Shared;
using Texts.Output;

public class AddListingScene : IScene
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly IListingService _listingService;
    private readonly ILogger<AddListingScene> _logger;
    private readonly ISceneStorage _storage;

    public AddListingScene(ISceneStorage storage, ILogger<AddListingScene> logger, ICallbackGenerator callbackGenerator,
                           IListingService listingService, ICallbackKeyboardGenerator callbackKeyboardGenerator)
    {
        _storage = storage;
        _logger = logger;
        _callbackGenerator = callbackGenerator;
        _listingService = listingService;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
    }

    public string SceneName => SceneNames.AddListingScene;

    public async Task EnterAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        await bot.SendMessage(userId, ResponseMessageStaticTexts.OnAddListing, ParseMode.Html,
            cancellationToken: cancellationToken);

        AddListingSceneContext context = await _storage.GetOrCreateSceneContextAsync(
            userId,
            SceneName,
            () => new AddListingSceneContext
            {
                UserId = userId,
                SceneName = SceneName,
                CurrentStep = AddListingSceneSteps.Ready
            },
            cancellationToken);

        context.CurrentStep = AddListingSceneSteps.NameEntering;
        context.UpdatedAt = DateTime.UtcNow;

        await _storage.SaveSceneContextAsync(userId, SceneName, context, cancellationToken);
    }

    public async Task HandleMessageAsync(long userId, Message message, ITelegramBotClient bot,
                                         CancellationToken cancellationToken)
    {
        AddListingSceneContext? context =
            await _storage.GetSceneContextAsync<AddListingSceneContext>(userId, SceneName, cancellationToken);
        if (context == null)
        {
            _logger.LogWarning("Unexpected scene context {SceneName}", SceneName);
            return;
        }

        switch (context.CurrentStep)
        {
            case AddListingSceneSteps.NameEntering:
                await HandleNameChoosingAsync(context, message, bot, cancellationToken);
                break;
            case AddListingSceneSteps.PriceEntering:
                await HandlePriceChoosingAsync(context, message, bot, cancellationToken);
                break;
            default:
                _logger.LogWarning("Unexpected step {Step} for user {UserId}", context.CurrentStep, userId);
                break;
        }
    }

    public async Task HandleCallbackAsync(long userId, CallbackQuery callback, ITelegramBotClient bot,
                                          CancellationToken cancellationToken)
    {
        if (callback.Data == null) return;

        AddListingSceneContext? context =
            await _storage.GetSceneContextAsync<AddListingSceneContext>(userId, SceneName, cancellationToken);
        if (context == null) return;

        Match addListingConfigurationMatch = _callbackGenerator.GetCallbackRegexOnConfirmListingPublication()
                                                               .Match(callback.Data);

        if (addListingConfigurationMatch.Success && context.CurrentStep == AddListingSceneSteps.Completed)
            await HandleAddListingConfirmationAsync(context, addListingConfigurationMatch, bot, cancellationToken);
    }

    public Task ExitAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {UserId} exiting AddListing scene", userId);
        return Task.CompletedTask;
    }

    private async Task HandleAddListingConfirmationAsync(AddListingSceneContext context, Match message,
                                                         ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        // TODO: Implement
        AddListingContract contract =
            AddListingContract.Create(context.UserId, context.SceneName, string.Empty, context.Price,
                Guid.NewGuid(), []);

        bool isCreated = await _listingService.AddListingAsync(contract, cancellationToken);
        string responseText = isCreated
            ? ResponseMessageStaticTexts.OnSuccessfulListingPublication
            : ResponseMessageStaticTexts.OnFailedListingPublication;

        await bot.SendMessage(contract.TelegramUserId, responseText, ParseMode.Html,
            cancellationToken: cancellationToken);

        await ExitAsync(context.UserId, bot, cancellationToken);
    }

    private async Task HandlePriceChoosingAsync(AddListingSceneContext context, Message message, ITelegramBotClient bot,
                                                CancellationToken cancellationToken)
    {
        if (message.Text == null)
        {
            _logger.LogWarning("Message text is null");
            return;
        }

        if (!decimal.TryParse(message.Text, out decimal price))
        {
            _logger.LogWarning("Price is not a number");
            return;
        }

        context.CurrentStep = AddListingSceneSteps.Completed;
        context.Price = price;
        context.UpdatedAt = DateTime.UtcNow;

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateInlineKeyboardMarkup(CallbackGenerationCodes
               .OnConfirmListingPublication);
        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingPriceCompleted,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
    }

    private async Task HandleNameChoosingAsync(AddListingSceneContext context, Message message, ITelegramBotClient bot,
                                               CancellationToken cancellationToken)
    {
        if (message.Text == null)
        {
            _logger.LogWarning("Message text is null");
            return;
        }

        string name = message.Text;
        context.CurrentStep = AddListingSceneSteps.PriceEntering;
        context.Name = name;
        context.UpdatedAt = DateTime.UtcNow;

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnListingPriceEntering,
            ParseMode.Html, cancellationToken: cancellationToken);
    }
}