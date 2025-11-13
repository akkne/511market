namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Implementation;

using Abstract;
using Core.Models;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using global::Telegram.Bot.Types.Enums;
using global::Telegram.Bot.Types.ReplyMarkups;
using Helpers.Abstract;
using ResaleTelegramBot.Services.Abstract;
using Texts.Output;

public class SelectingSearchingOptionsCallbackHandler : ICallbackHandler
{
    private readonly ICallbackGenerator _callbackGenerator;
    private readonly ICallbackKeyboardGenerator _callbackKeyboardGenerator;
    private readonly ICategoryService _categoryService;

    public SelectingSearchingOptionsCallbackHandler(ICallbackGenerator callbackGenerator,
                                                    ICallbackKeyboardGenerator callbackKeyboardGenerator,
                                                    ICategoryService categoryService)
    {
        _callbackGenerator = callbackGenerator;
        _callbackKeyboardGenerator = callbackKeyboardGenerator;
        _categoryService = categoryService;
    }

    public bool CanHandle(CallbackQuery callbackQuery)
    {
        if (callbackQuery.Data == null) return false;

        string data = callbackQuery.Data;

        return _callbackGenerator.GetCallbackRegexOnSearchByCategory().IsMatch(data);
    }

    public async Task HandleCallbackAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                          CancellationToken cancellationToken)
    {
        if (callbackQuery.Data == null) return;

        string data = callbackQuery.Data;

        if (_callbackGenerator.GetCallbackRegexOnSearchByCategory().IsMatch(data))
            await HandleSearchByCategoryAsync(callbackQuery, botClient, cancellationToken);
    }

    private async Task HandleSearchByCategoryAsync(CallbackQuery callbackQuery, ITelegramBotClient botClient,
                                                   CancellationToken cancellationToken)
    {
        List<Category> categories = await _categoryService.GetCategoriesAsync(cancellationToken);
        InlineKeyboardMarkup keyboardMarkup =
            _callbackKeyboardGenerator.GenerateOnCategorySelectionForSearch(categories);

        await botClient.EditMessageText(callbackQuery.From.Id, callbackQuery.Message!.MessageId,
            ResponseMessageStaticTexts.OnCategorySelectionForSearch, ParseMode.Html, keyboardMarkup,
            cancellationToken: cancellationToken);
    }
}