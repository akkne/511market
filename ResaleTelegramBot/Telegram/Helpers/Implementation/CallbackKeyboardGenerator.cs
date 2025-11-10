namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using Abstract;
using global::Telegram.Bot.Types.ReplyMarkups;
using Shared.Enums;
using Texts.Input;

public class CallbackKeyboardGenerator : ICallbackKeyboardGenerator
{
    private readonly ICallbackGenerator _callbackGenerator;

    public CallbackKeyboardGenerator(ICallbackGenerator callbackGenerator)
    {
        _callbackGenerator = callbackGenerator;
    }

    public InlineKeyboardMarkup GenerateInlineKeyboardMarkup(CallbackGenerationCodes code)
    {
        return code switch
        {
            CallbackGenerationCodes.OnConfirmListingPublication =>
                new InlineKeyboardMarkup(
                    new InlineKeyboardButton(CallbackKeyboardStaticTexts.OnListingPublicationConfirm)
                    {
                        CallbackData = _callbackGenerator.GenerateCallbackRegexOnConfirmListingPublication()
                    }),
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}