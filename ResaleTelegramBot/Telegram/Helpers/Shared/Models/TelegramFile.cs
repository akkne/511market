namespace ResaleTelegramBot.Telegram.Helpers.Shared.Models;

using Enums;
using global::Telegram.Bot.Types;

public class TelegramFile
{
    public InputFile File { get; set; }
    public FileTypes FileType { get; set; }

    public static TelegramFile Create(InputFile file, FileTypes fileType)
    {
        return new TelegramFile
        {
            File = file,
            FileType = fileType
        };
    }
}