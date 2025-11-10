namespace ResaleTelegramBot.Services.Abstract;

using Contracts.Auth;
using Core.Models;

public interface IAuthService
{
    Task<User?> RegisterUserAsync(UserRegistrationContract userRegistration,
                                  CancellationToken cancellationToken = default);

    Task<bool> ContainsUserAsync(long telegramId, CancellationToken cancellationToken = default);
    Task<User?> GetByTelegramIdAsync(long telegramId, CancellationToken cancellationToken = default);
}