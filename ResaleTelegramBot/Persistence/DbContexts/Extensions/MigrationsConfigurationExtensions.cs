namespace ResaleTelegramBot.Persistence.DbContexts.Extensions;

public static class MigrationsConfigurationExtensions
{
    public static async Task<WebApplication> ConfigureMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        ApplicationDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await applicationDbContext.Database.EnsureCreatedAsync();

        return app;
    }
}