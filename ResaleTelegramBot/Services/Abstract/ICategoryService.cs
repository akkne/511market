namespace ResaleTelegramBot.Services.Abstract;

using Core.Models;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
    Task<bool> ContainsByIdAsync(Guid categoryId, CancellationToken cancellationToken);
    Task<Category?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken);
}