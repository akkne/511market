using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResaleTelegramBot.Persistence.DbContexts;
using ResaleTelegramBot.Persistence.DbContexts.Extensions;
using ResaleTelegramBot.Persistence.Scenes.Abstract;
using ResaleTelegramBot.Persistence.Scenes.Implementation;
using ResaleTelegramBot.Persistence.Scenes.Options;
using ResaleTelegramBot.Services.Abstract;
using ResaleTelegramBot.Services.Implementation;
using ResaleTelegramBot.Telegram.Helpers.Abstract;
using ResaleTelegramBot.Telegram.Helpers.Implementation;
using ResaleTelegramBot.Telegram.Scenes.Gateway.Abstract;
using ResaleTelegramBot.Telegram.Scenes.Gateway.Implementation;
using ResaleTelegramBot.Telegram.Scenes.Managers.Abstract;
using ResaleTelegramBot.Telegram.Scenes.Managers.Implementation;
using ResaleTelegramBot.Telegram.Scenes.Scenes.Abstract;
using ResaleTelegramBot.Telegram.Scenes.Scenes.Implementation;
using ResaleTelegramBot.Telegram.Services.Abstract;
using ResaleTelegramBot.Telegram.Services.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.BaseHandlers.RouterServices.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Callback.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.Command.Implementation;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.RegularText.Abstract;
using ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.RegularText.Implementation;
using ResaleTelegramBot.Telegram.Webhooks.Configuration;
using ResaleTelegramBot.Telegram.Webhooks.Endpoints;
using ResaleTelegramBot.Telegram.Webhooks.Services.Abstract;
using ResaleTelegramBot.Telegram.Webhooks.Services.Hosted;
using ResaleTelegramBot.Telegram.Webhooks.Services.Implementation;
using ResaleTelegramBot.Telegram.Webhooks.Services.Options;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Polling;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);

WebApplication app = builder.Build();

app.MapWebhookEndpoints();

await app.ConfigureMigrations();

app.Run();

return;

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<RedisSceneStorageConfiguration>(
        configuration.GetSection(RedisSceneStorageConfiguration.SectionName));

    services.AddDbContext<ApplicationDbContext>(options =>
    {
        string connectionString = configuration.GetConnectionString(nameof(ApplicationDbContext))
                               ?? throw new NullReferenceException("No connection string found.");

        options.UseNpgsql(connectionString)
               .EnableSensitiveDataLogging();
    });

    services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
    {
        RedisSceneStorageConfiguration configurationOptions =
            serviceProvider.GetService<IOptions<RedisSceneStorageConfiguration>>()?.Value
         ?? throw new Exception("No RedisSceneStorageConfiguration configured");

        ConfigurationOptions options = ConfigurationOptions.Parse(configurationOptions.ConnectionString);
        options.AbortOnConnectFail = configurationOptions.AbortOnConnectFail;

        return ConnectionMultiplexer.Connect(options);
    });

    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IListingService, ListingService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<IFavoriteService, FavoriteService>();
    services.AddScoped<IListingSearchService, ListingSearchService>();

    services.AddSingleton<ISceneStorage, RedisSceneStorage>();

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

    services.AddScoped<IRegularTextHandler, AddListingRegularTextHandler>();
    services.AddScoped<IRegularTextHandler, FindListingsRegularTextHandler>();

    services.AddScoped<ICallbackHandler, SearchByCategoryCallbackHandler>();
    services.AddScoped<ICallbackHandler, SelectingSearchingOptionsCallbackHandler>();
    services.AddScoped<ICallbackHandler, ToggleFavoriteCallbackHandler>();

    services.AddScoped<ICommandHandler, StartCommandHandler>();

    services.AddTransient<IReplyKeyboardGenerator, ReplyKeyboardGenerator>();

    services.AddScoped<ISceneManager, SceneManager>();

    services.AddScoped<ISceneGatewayService, SceneGatewayService>();

    services.AddScoped<IScene, AddListingScene>();

    services.AddTransient<ICallbackKeyboardGenerator, CallbackKeyboardGenerator>();
    services.AddTransient<ICallbackGenerator, CallbackGenerator>();

    services.AddScoped<IListingViewService, ListingViewService>();
}