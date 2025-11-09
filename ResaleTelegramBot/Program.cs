using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResaleTelegramBot.Persistence.DbContexts;
using ResaleTelegramBot.Services.Abstract;
using ResaleTelegramBot.Services.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Implementation;
using ResaleTelegramBot.Telegram.Webhooks.Configuration;
using ResaleTelegramBot.Telegram.Webhooks.Endpoints;
using ResaleTelegramBot.Telegram.Webhooks.Services.Abstract;
using ResaleTelegramBot.Telegram.Webhooks.Services.Hosted;
using ResaleTelegramBot.Telegram.Webhooks.Services.Implementation;
using ResaleTelegramBot.Telegram.Webhooks.Services.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

app.MapWebhookEndpoints();

app.Run();

return;

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        string connectionString = configuration.GetConnectionString(nameof(ApplicationDbContext))
                               ?? throw new NullReferenceException("No connection string found.");

        options.UseNpgsql(connectionString)
               .EnableSensitiveDataLogging();
    });

    services.AddScoped<IAuthService, AuthService>();

    ConfigureTelegramServices(services, configuration);
}

void ConfigureTelegramServices(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<WebhookConfigurationOptions>(
        configuration.GetSection(WebhookConfigurationOptions.SectionName));

    services.Configure<TelegramConfigurationOptions>(
        configuration.GetSection(TelegramConfigurationOptions.SectionName));


    services.AddHttpClient(nameof(TelegramBotClient))
            .AddTypedClient<ITelegramBotClient>((client, provider) =>
             {
                 IOptions<TelegramConfigurationOptions> configurationOptions =
                     provider.GetService<IOptions<TelegramConfigurationOptions>>()
                  ?? throw new ArgumentNullException(nameof(configurationOptions));

                 return new TelegramBotClient(configurationOptions.Value.TelegramBotToken);
             });

    services.AddScoped<IUpdateHandler, BaseUpdatesHandler>();

    services.AddSingleton<IWebhookConfigurationService, WebhookConfigurationService>();
    services.AddHostedService<WebhookReceiverService>();

    services.AddScoped<ICallbackRouterService, CallbackRouterService>();
    services.AddScoped<ICommandRouterService, CommandRouterService>();
    services.AddScoped<IRegularTextRouterService, RegularTextRouterService>();
    services.AddScoped<IStateTextRouterService, StateTextRouterService>();

    services.AddScoped<ICommandHandler, StartCommandHandler>();
}