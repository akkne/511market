namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Contracts.Listing;

public class ListingService : IListingService
{
    public async Task<bool> AddListingAsync(AddListingContract contract, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}