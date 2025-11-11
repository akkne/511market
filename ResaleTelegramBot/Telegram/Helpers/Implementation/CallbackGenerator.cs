namespace ResaleTelegramBot.Telegram.Helpers.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Shared.Enums;

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

    public Regex GetCallbackRegexOnChoosingCategoryOnAddingListing()
    {
        return new Regex($"listing/adding/category/(?<{CallbackGenerationStaticStrings.CategoryId}>.+)");
    }

    public string GenerateCallbackRegexOnChoosingCategoryOnAddingListing(string categoryGuid)
    {
        return $"listing/adding/category/{categoryGuid}";
    }
}