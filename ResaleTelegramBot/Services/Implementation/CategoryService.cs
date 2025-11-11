namespace ResaleTelegramBot.Services.Implementation;

using Abstract;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Persistence.DbContexts;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ApplicationDbContext dbContext, ILogger<CategoryService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.ToListAsync(cancellationToken);
    }

    public async Task<bool> ContainsByIdAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.AnyAsync(x => x.Id == categoryId, cancellationToken);
    }

    public async Task<Category?> GetCategoryAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.FirstOrDefaultAsync(x => x.Id == categoryId, cancellationToken);
    }
}