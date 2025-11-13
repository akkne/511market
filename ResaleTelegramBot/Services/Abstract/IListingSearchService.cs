namespace ResaleTelegramBot.Services.Abstract;

using Core.Models;

public interface IListingSearchService
{
    Task<List<Listing>> GetListingsAsync(Guid? categoryId, string searchingText,
                                         CancellationToken cancellationToken = default);
}