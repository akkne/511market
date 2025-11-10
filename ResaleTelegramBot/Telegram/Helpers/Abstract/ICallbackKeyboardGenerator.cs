namespace ResaleTelegramBot.Telegram.Helpers.Abstract;

using global::Telegram.Bot.Types.ReplyMarkups;
using Shared.Enums;

public interface ICallbackKeyboardGenerator
{
    InlineKeyboardMarkup GenerateInlineKeyboardMarkup(CallbackGenerationCodes code);
}