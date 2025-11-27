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

    public Regex GetCallbackRegexOnFinishPhotoUploading()
    {
        return new Regex("listing/adding/photo/finish");
    }

    public string GenerateCallbackRegexOnFinishPhotoUploading()
    {
        return "listing/adding/photo/finish";
    }

    public Regex GetCallbackRegexOnSearchByCategory()
    {
        return new Regex("search/by/category");
    }

    public string GenerateCallbackRegexOnSearchByCategory()
    {
        return "search/by/category";
    }

    public Regex GetCallbackRegexOnSearchByText()
    {
        return new Regex("search/by/text");
    }

    public string GenerateCallbackRegexOnSearchByText()
    {
        return "search/by/text";
    }

    public Regex GetCallbackRegexOnSelectCategoryForSearch()
    {
        return new Regex($"search/category/(?<{CallbackGenerationStaticStrings.CategoryId}>.+)");
    }

    public string GenerateCallbackRegexOnSelectCategoryForSearch(string categoryGuid)
    {
        return $"search/category/{categoryGuid}";
    }

    public Regex GetCallbackRegexOnViewListing()
    {
        return new Regex(
            $"search/listing/(?<{CallbackGenerationStaticStrings.CategoryId}>[^/]+)/(?<{CallbackGenerationStaticStrings.SearchText}>[^/]+)/(?<{CallbackGenerationStaticStrings.ListingIndex}>\\d+)/(?<{CallbackGenerationStaticStrings.TotalListings}>\\d+)");
    }

    public string GenerateCallbackRegexOnViewListing(Guid? categoryId, string searchText, int listingIndex,
                                                     int totalListings)
    {
        string categoryIdString = categoryId?.ToString() ?? "null";
        string escapedSearchText = Uri.EscapeDataString(searchText);
        return $"search/listing/{categoryIdString}/{escapedSearchText}/{listingIndex}/{totalListings}";
    }

    public Regex GetCallbackRegexOnToggleFavorite()
    {
        return new Regex($"search/favorite/(?<{CallbackGenerationStaticStrings.ListingId}>.+)");
    }

    public string GenerateCallbackRegexOnToggleFavorite(Guid listingId)
    {
        return $"search/favorite/{listingId}";
    }

    public Regex GetCallbackRegexOnReportListing()
    {
        return new Regex($"search/report/(?<{CallbackGenerationStaticStrings.ListingId}>.+)");
    }

    public string GenerateCallbackRegexOnReportListing(Guid listingId)
    {
        return $"search/report/{listingId}";
    }

    public Regex GetCallbackRegexOnViewLongListing()
    {
        return new Regex(
            $"listing/long/(?<{CallbackGenerationStaticStrings.ListingId}>[^/]+)/(?<{CallbackGenerationStaticStrings.SearchText}>[^/]+)/(?<{CallbackGenerationStaticStrings.ListingIndex}>\\d+)/(?<{CallbackGenerationStaticStrings.TotalListings}>\\d+)");
    }

    public string GenerateCallbackRegexOnViewLongListing(Guid listingId, string searchText,
                                                         int startIndex, int totalListings)
    {
        string escapedSearchText = Uri.EscapeDataString(searchText);
        return $"listing/long/{listingId}/{escapedSearchText}/{startIndex}/{totalListings}";
    }
}