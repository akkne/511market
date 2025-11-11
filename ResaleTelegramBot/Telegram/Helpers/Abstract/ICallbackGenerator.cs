namespace ResaleTelegramBot.Telegram.Helpers.Abstract;

using System.Text.RegularExpressions;

public interface ICallbackGenerator
{
    Regex GetCallbackRegexOnConfirmListingPublication();
    string GenerateCallbackRegexOnConfirmListingPublication();

    Regex GetCallbackRegexOnChoosingCategoryOnAddingListing();
    string GenerateCallbackRegexOnChoosingCategoryOnAddingListing(string categoryGuid);

    Regex GetCallbackRegexOnFinishPhotoUploading();
    string GenerateCallbackRegexOnFinishPhotoUploading();
}