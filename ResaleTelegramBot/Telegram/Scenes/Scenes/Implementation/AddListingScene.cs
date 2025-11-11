namespace ResaleTelegramBot.Telegram.Scenes.Scenes.Implementation;

using System.Linq;
using System.Text.RegularExpressions;
using Abstract;
using Contexts.Implementation;
using Contexts.SceneSteps;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using Helpers.Shared.Enums;
using Persistence.Scenes.Abstract;
using Services.Abstract;
using Services.Contracts.Listing;
using Services.Contracts.Listing.Models;
using Shared;
using Texts.Output;

public class AddListingScene : IScene
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ICategoryService _categoryService;
    private readonly IListingService _listingService;
    private readonly ILogger<AddListingScene> _logger;
    private readonly ISceneStorage _storage;

    public AddListingScene(ISceneStorage storage, ILogger<AddListingScene> logger, ICallbackGenerator callbackGenerator,
                           IListingService listingService, ICallbackKeyboardGenerator callbackKeyboardGenerator,
                           ICategoryService categoryService)
    {
        _storage = storage;
        _logger = logger;
        _callbackGenerator = callbackGenerator;
        _listingService = listingService;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
        _categoryService = categoryService;
    }

    public string SceneName => SceneNames.AddListingScene;

    public async Task EnterAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
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

        List<Category> categories = await _categoryService.GetCategoriesAsync(cancellationToken);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnChoosingCategoryOnAddingListing(categories);

        Message message = await bot.SendMessage(context.UserId,
            ResponseMessageStaticTexts.OnCategoryChoosingOnAddingListing,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);

        context.CurrentStep = AddListingSceneSteps.CategoryChoosing;
        context.LastMessageId = message.MessageId;

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
            case AddListingSceneSteps.DescriptionEntering:
                await HandleDescriptionEnteringAsync(context, message, bot, cancellationToken);
                break;
            case AddListingSceneSteps.PhotoUploading:
                await HandlePhotoUploadingAsync(context, message, bot, cancellationToken);
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
        {
            await HandleAddListingConfirmationAsync(context, addListingConfigurationMatch, bot, cancellationToken);
            return;
        }

        Match addCategoryOnAddListingConfigurationMatch = _callbackGenerator
                                                         .GetCallbackRegexOnChoosingCategoryOnAddingListing()
                                                         .Match(callback.Data);

        if (addCategoryOnAddListingConfigurationMatch.Success &&
            context.CurrentStep == AddListingSceneSteps.CategoryChoosing)
            await HandleChooseCategoryAddListingConfirmationAsync(context, addCategoryOnAddListingConfigurationMatch,
                bot, cancellationToken);

        Match finishPhotoUploadingMatch = _callbackGenerator.GetCallbackRegexOnFinishPhotoUploading()
                                                            .Match(callback.Data);

        if (finishPhotoUploadingMatch.Success && context.CurrentStep == AddListingSceneSteps.PhotoUploading)
            await HandleFinishPhotoUploadingAsync(context, bot, cancellationToken);
    }

    public Task ExitAsync(long userId, ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        _logger.LogInformation("User {UserId} exiting AddListing scene", userId);
        return Task.CompletedTask;
    }

    private async Task HandleChooseCategoryAddListingConfirmationAsync(AddListingSceneContext context, Match match,
                                                                       ITelegramBotClient bot,
                                                                       CancellationToken cancellationToken)
    {
        string stringGuid = match.Groups[CallbackGenerationStaticStrings.CategoryId].Value;
        if (!Guid.TryParse(stringGuid, out Guid categoryGuid))
        {
            _logger.LogWarning("Category Guid: {guid} is not a Guid", stringGuid);
            return;
        }

        bool exists = await _categoryService.ContainsByIdAsync(categoryGuid, cancellationToken);
        if (!exists)
        {
            _logger.LogWarning("Category {CategoryId} doesn't exist", categoryGuid);
            return;
        }

        context.CurrentStep = AddListingSceneSteps.NameEntering;
        context.CategoryId = categoryGuid;
        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        await bot.EditMessageText(context.UserId, context.LastMessageId, ResponseMessageStaticTexts.OnNameEntering,
            ParseMode.Html, InlineKeyboardMarkup.Empty(), cancellationToken: cancellationToken);
    }

    private async Task HandleAddListingConfirmationAsync(AddListingSceneContext context, Match match,
                                                         ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        List<ListingPhotosModel> photos = context.Photos
            .Select((fileId, index) => new ListingPhotosModel
            {
                TelegramFileId = fileId,
                Order = index
            })
            .ToList();

        AddListingContract contract =
            AddListingContract.Create(context.UserId, context.Name, context.Description, context.Price,
                context.CategoryId, photos);

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

        context.CurrentStep = AddListingSceneSteps.DescriptionEntering;
        context.Price = price;

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingDescription,
            ParseMode.Html, cancellationToken: cancellationToken);
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

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnListingPriceEntering,
            ParseMode.Html, cancellationToken: cancellationToken);
    }

    private async Task HandleDescriptionEnteringAsync(AddListingSceneContext context, Message message,
                                                      ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        if (message.Text == null)
        {
            _logger.LogWarning("Message text is null");
            return;
        }

        string description = message.Text;
        context.CurrentStep = AddListingSceneSteps.PhotoUploading;
        context.Description = description;

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnFinishPhotoUploading();

        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingPhotoUploading,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
    }

    private async Task HandlePhotoUploadingAsync(AddListingSceneContext context, Message message,
                                                 ITelegramBotClient bot, CancellationToken cancellationToken)
    {
        if (message.Photo == null || message.Photo.Length == 0)
        {
            _logger.LogWarning("Message photo is null or empty");
            return;
        }

        const int maxPhotos = 10;
        if (context.Photos.Count >= maxPhotos)
        {
            InlineKeyboardMarkup keyboardMarkup =
                _callbackKeyboardGenerator.GenerateOnFinishPhotoUploading();
            await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingPhotoLimitReached,
                ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
            return;
        }

        PhotoSize[] photos = message.Photo;
        string fileId = photos[^1].FileId;

        context.Photos.Add(fileId);

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        InlineKeyboardMarkup inlineKeyboard =
            _callbackKeyboardGenerator.GenerateOnFinishPhotoUploading();

        if (context.Photos.Count >= maxPhotos)
        {
            await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingPhotoLimitReached,
                ParseMode.Html, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }
        else
        {
            await bot.SendMessage(context.UserId,
                $"{ResponseMessageStaticTexts.OnAddListingPhotoUploaded}\nЗагружено фото: {context.Photos.Count}/{maxPhotos}",
                ParseMode.Html, replyMarkup: inlineKeyboard, cancellationToken: cancellationToken);
        }
    }

    private async Task HandleFinishPhotoUploadingAsync(AddListingSceneContext context, ITelegramBotClient bot,
                                                       CancellationToken cancellationToken)
    {
        context.CurrentStep = AddListingSceneSteps.Completed;

        await _storage.SaveSceneContextAsync(context.UserId, SceneName, context, cancellationToken);

        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnConfirmListingPublication();

        await bot.SendMessage(context.UserId, ResponseMessageStaticTexts.OnAddListingCompleted,
            ParseMode.Html, replyMarkup: keyboardMarkup, cancellationToken: cancellationToken);
    }
}