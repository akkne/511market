namespace ResaleTelegramBot.Telegram.Scenes.Contexts.Implementation;

using Abstract;
using SceneSteps;

public class AddListingSceneContext : BaseSceneContext
{
    public AddListingSceneSteps CurrentStep { get; set; } = AddListingSceneSteps.Ready;
    public string Name { get; set; }
    public decimal Price { get; set; }
}