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

    Regex GetCallbackRegexOnSearchByCategory();
    string GenerateCallbackRegexOnSearchByCategory();

    Regex GetCallbackRegexOnSearchByText();
    string GenerateCallbackRegexOnSearchByText();

    Regex GetCallbackRegexOnSelectCategoryForSearch();
    string GenerateCallbackRegexOnSelectCategoryForSearch(string categoryGuid);

    Regex GetCallbackRegexOnViewListing();
    string GenerateCallbackRegexOnViewListing(Guid? categoryId, string searchText, int listingIndex, int totalListings);

    Regex GetCallbackRegexOnToggleFavorite();
    string GenerateCallbackRegexOnToggleFavorite(Guid listingId);

    Regex GetCallbackRegexOnReportListing();
    string GenerateCallbackRegexOnReportListing(Guid listingId);
}