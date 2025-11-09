namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using Abstract;
using global::Telegram.Bot.Types.ReplyMarkups;
using Shared.Enums;
using Texts.Input;

public class ReplyKeyboardGenerator : IReplyKeyboardGenerator
{
    public ReplyMarkup GenerateKeyboardMarkup(KeyboardGenerationCodes code)
    {
        return code switch
        {
            KeyboardGenerationCodes.MainMenu => new ReplyKeyboardMarkup([
                [RegularTextStaticTexts.FindListings, RegularTextStaticTexts.Favorite],
                [RegularTextStaticTexts.MyProfile, RegularTextStaticTexts.AddListing],
                [RegularTextStaticTexts.Settings]
            ])
            {
                ResizeKeyboard = true
            },
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, null)
        };
    }
}