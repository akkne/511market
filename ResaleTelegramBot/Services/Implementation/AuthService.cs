namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Contracts.Auth;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.DbContexts;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    public AuthService(ApplicationDbContext dbContext, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<User?> RegisterUserAsync(UserRegistrationContract userRegistrationContract,
                                               CancellationToken cancellationToken = default)
    {
        User created = User.Create(userRegistrationContract.TelegramData);

        try
        {
            await using IDbContextTransaction transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await _dbContext.Users.AddAsync(created, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to create user");
            return null;
        }

        return created;
    }

    public async Task<bool> ContainsUserAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.AnyAsync(x => x.TelegramData.Id == telegramId, cancellationToken);
    }

    public async Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
                               .AsNoTracking()
                               .FirstOrDefaultAsync(x => x.TelegramData.Id == telegramId, cancellationToken);
    }
}