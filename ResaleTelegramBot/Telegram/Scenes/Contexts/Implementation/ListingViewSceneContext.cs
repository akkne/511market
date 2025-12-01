namespace ResaleTelegramBot.Telegram.Scenes.Contexts.Implementation;

using Abstract;

public class ListingViewSceneContext : BaseSceneContext
{
    public List<int>? MediaGroupMessageId { get; set; }
    public int? ButtonsMessageId { get; set; }

    public static ListingViewSceneContext CreateEmpty()
    {
        return new ListingViewSceneContext
        {
            MediaGroupMessageId = null,
            ButtonsMessageId = null
        };
    }
}