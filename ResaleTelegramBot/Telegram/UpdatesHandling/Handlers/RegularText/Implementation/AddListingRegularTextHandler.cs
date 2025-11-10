namespace ResaleTelegramBot.Telegram.UpdatesHandling.Handlers.RegularText.Implementation;

using Abstract;
using global::Telegram.Bot;
using global::Telegram.Bot.Types;
using Scenes.Gateway.Abstract;
using Scenes.Shared;
using Texts.Input;

public class AddListingRegularTextHandler : IRegularTextHandler
{
    private readonly ILogger<AddListingRegularTextHandler> _logger;
    private readonly ISceneGatewayService _sceneGatewayService;

    public AddListingRegularTextHandler(ILogger<AddListingRegularTextHandler> logger,
                                        ISceneGatewayService sceneGatewayService)
    {
        _logger = logger;
        _sceneGatewayService = sceneGatewayService;
    }

    public bool CanHandle(string message)
    {
        return message == RegularTextStaticTexts.AddListing;
    }

    public async Task HandleRegularTextAsync(Message message, ITelegramBotClient botClient,
                                             CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            _logger.LogWarning("From is null");
            return;
        }

        await _sceneGatewayService.EnterSceneAsync(message.From.Id, SceneNames.AddListingScene, botClient,
            cancellationToken);
    }
}