namespace ResaleTelegramBot.Services.Abstract;

using Contracts.Listing;

public interface IListingService
{
    Task<bool> AddListingAsync(AddListingContract contract, CancellationToken cancellationToken = default);
}