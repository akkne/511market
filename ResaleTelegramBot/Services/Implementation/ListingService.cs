namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Contracts.Listing;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.DbContexts;

public class ListingService : IListingService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ListingService> _logger;

    public ListingService(ApplicationDbContext dbContext, ILogger<ListingService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AddListingAsync(AddListingContract contract, CancellationToken cancellationToken = default)
    {
        SellerProfile seller = await _dbContext.SellerProfiles
                                               .Include(x => x.UserProfile)
                                               .FirstOrDefaultAsync(x =>
                                                        x.UserProfile.TelegramData.Id == contract.TelegramUserId,
                                                    cancellationToken)
                            ?? throw new NullReferenceException("UserProfile not found");

        Category category = await _dbContext.Categories
                                            .FirstOrDefaultAsync(x => x.Id == contract.CategoryId, cancellationToken)
                         ?? throw new NullReferenceException("Category not found");

        Listing created = Listing.Create(contract.Name, contract.Description, contract.Price, seller, category,
            contract.Photos);

        try
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _dbContext.Listings.AddAsync(created, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create user");
            return false;
        }
    }
}