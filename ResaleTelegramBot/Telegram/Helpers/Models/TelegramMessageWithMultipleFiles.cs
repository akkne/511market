namespace ResaleTelegramBot.Telegram.Helpers.Models;

using Shared.Models;

public class TelegramMessageWithMultipleFiles
{
    public string Text { get; set; }
    public List<TelegramFile> Files { get; set; }

    public static TelegramMessageWithMultipleFiles Create(string text, List<TelegramFile> files)
    {
        return new TelegramMessageWithMultipleFiles { Text = text, Files = files };
    }
}