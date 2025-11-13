namespace ResaleTelegramBot.Services.Abstract;

using Contracts.Listing;
using Core.Models;

public interface IListingService
{
    Task<bool> AddListingAsync(AddListingContract contract, CancellationToken cancellationToken = default);
    Task<Listing?> GetListingByIdAsync(Guid listingId, CancellationToken cancellationToken = default);
}