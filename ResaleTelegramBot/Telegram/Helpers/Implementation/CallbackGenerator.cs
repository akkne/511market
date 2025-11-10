namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using System.Text.RegularExpressions;
using Abstract;

public class CallbackGenerator : ICallbackGenerator
{
    public Regex GetCallbackRegexOnConfirmListingPublication()
    {
        return new Regex("listing/publication/confirm");
    }

    public string GenerateCallbackRegexOnConfirmListingPublication()
    {
        return "listing/publication/confirm";
    }
}