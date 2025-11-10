namespace ResaleTelegramBot.Telegram.Helpers.Abstract;

using System.Text.RegularExpressions;

public interface ICallbackGenerator
{
    Regex GetCallbackRegexOnConfirmListingPublication();
    string GenerateCallbackRegexOnConfirmListingPublication();
}