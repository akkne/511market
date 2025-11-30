namespace ResaleTelegramBot.Telegram.Scenes.Contexts.Implementation;

using Abstract;

public class ListingViewSceneContext : BaseSceneContext
{
    public ListingViewSceneContext()
    {
        MediaGroupMessageId = 0;
        ButtonsMessageId = 0;
    }

    public int MediaGroupMessageId { get; set; }
    public int ButtonsMessageId { get; set; }
}