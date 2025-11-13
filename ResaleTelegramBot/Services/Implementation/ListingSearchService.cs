namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Core.Models;
using Core.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContexts;

public class ListingSearchService : IListingSearchService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ListingSearchService> _logger;

    public ListingSearchService(ApplicationDbContext dbContext, ILogger<ListingSearchService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Listing>> GetListingsAsync(Guid? categoryId, string searchingText,
                                                      CancellationToken cancellationToken = default)
    {
        if (categoryId == null)
        {
            _logger.LogInformation("CategoryId is null");
            return await GetListingsWithoutCategoryInfoAsync(searchingText, cancellationToken);
        }

        return await GetListingsWithCategoryInfoAsync(categoryId.Value, searchingText, cancellationToken);
    }

    private async Task<List<Listing>> GetListingsWithoutCategoryInfoAsync(
        string searchingText, CancellationToken cancellationToken = default)
    {
        string lowerSearchText = searchingText.ToLower();
        return await _dbContext.Listings
                               .Include(x => x.Category)
                               .Include(x => x.Photos.OrderBy(p => p.Order))
                               .Include(x => x.SellerProfile)
                               .ThenInclude(x => x.UserProfile)
                               .Where(x => x.Status == ListingStatus.Active &&
                                           (x.Title.ToLower().Contains(lowerSearchText) ||
                                            x.Description.ToLower().Contains(lowerSearchText)))
                               .OrderByDescending(x => x.CreatedAt)
                               .ToListAsync(cancellationToken);
    }

    private async Task<List<Listing>> GetListingsWithCategoryInfoAsync(
        Guid categoryId, string searchingText, CancellationToken cancellationToken = default)
    {
        string lowerSearchText = searchingText.ToLower();
        return await _dbContext.Listings
                               .Include(x => x.Category)
                               .Include(x => x.Photos.OrderBy(p => p.Order))
                               .Include(x => x.SellerProfile)
                               .ThenInclude(x => x.UserProfile)
                               .Where(x => x.Status == ListingStatus.Active &&
                                           (x.Title.ToLower().Contains(lowerSearchText) ||
                                            x.Description.ToLower().Contains(lowerSearchText)) &&
                                           x.Category.Id == categoryId)
                               .OrderByDescending(x => x.CreatedAt)
                               .ToListAsync(cancellationToken);
    }
}