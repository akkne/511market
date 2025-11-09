namespace ResaleTelegramBot.Telegram.Helpers.Models;

using Shared.Models;

public class TelegramMessageWithOneFile
{
    public string Text { get; set; }
    public TelegramFile File { get; set; }

    public static TelegramMessageWithOneFile Create(string text, TelegramFile file)
    {
        return new TelegramMessageWithOneFile { Text = text, File = file };
    }
}