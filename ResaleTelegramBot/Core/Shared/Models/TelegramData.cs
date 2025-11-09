namespace ResaleTelegramBot.Core.Shared.Models;

public class TelegramData
{
    public long Id { get; set; }
    public string? Username { get; set; }

    public static TelegramData Create(long id, string? username)
    {
        return new TelegramData
        {
            Id = id,
            Username = username
        };
    }
}