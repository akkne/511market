namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.DbContexts;

public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(ApplicationDbContext dbContext, ILogger<FavoriteService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> AddFavoriteAsync(long telegramUserId, Guid listingId,
                                             CancellationToken cancellationToken = default)
    {
        User? user = await _dbContext.Users
                                     .Include(x => x.BuyerProfile)
                                     .FirstOrDefaultAsync(x => x.TelegramData.Id == telegramUserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User or BuyerProfile not found for telegramUserId: {TelegramUserId}", telegramUserId);
            return false;
        }

        bool alreadyExists = await _dbContext.Favorites
                                             .AnyAsync(x => x.BuyerProfile.Id == user.BuyerProfile.Id &&
                                                            x.Listing.Id == listingId, cancellationToken);

        if (alreadyExists) return true;

        Listing? listing = await _dbContext.Listings
                                           .FirstOrDefaultAsync(x => x.Id == listingId, cancellationToken);

        if (listing == null)
        {
            _logger.LogWarning("Listing not found: {ListingId}", listingId);
            return false;
        }

        Favorite favorite = new()
        {
            Id = Guid.NewGuid(),
            BuyerProfile = user.BuyerProfile,
            Listing = listing,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _dbContext.Favorites.AddAsync(favorite, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to add favorite");
            return false;
        }
    }

    public async Task<bool> RemoveFavoriteAsync(long telegramUserId, Guid listingId,
                                                CancellationToken cancellationToken = default)
    {
        User? user = await _dbContext.Users
                                     .Include(x => x.BuyerProfile)
                                     .FirstOrDefaultAsync(x => x.TelegramData.Id == telegramUserId, cancellationToken);

        if (user == null || user.BuyerProfile == null)
        {
            _logger.LogWarning("User or BuyerProfile not found for telegramUserId: {TelegramUserId}", telegramUserId);
            return false;
        }

        Favorite? favorite = await _dbContext.Favorites
                                             .FirstOrDefaultAsync(x => x.BuyerProfile.Id == user.BuyerProfile.Id &&
                                                                       x.Listing.Id == listingId, cancellationToken);

        if (favorite == null) return true;

        try
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            _dbContext.Favorites.Remove(favorite);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to remove favorite");
            return false;
        }
    }

    public async Task<bool> IsFavoriteAsync(long telegramUserId, Guid listingId,
                                            CancellationToken cancellationToken = default)
    {
        User? user = await _dbContext.Users
                                     .Include(x => x.BuyerProfile)
                                     .FirstOrDefaultAsync(x => x.TelegramData.Id == telegramUserId, cancellationToken);

        if (user == null || user.BuyerProfile == null) return false;

        return await _dbContext.Favorites
                               .AnyAsync(x => x.BuyerProfile.Id == user.BuyerProfile.Id &&
                                              x.Listing.Id == listingId, cancellationToken);
    }
}