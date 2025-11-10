namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Contracts.Listing;

public class ListingService : IListingService
{
    public Task<bool> AddListingAsync(AddListingContract contract, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}