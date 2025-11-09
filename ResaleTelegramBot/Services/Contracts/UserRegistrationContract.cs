namespace ResaleTelegramBot.Services.Contracts;

using Core.Shared.Models;

public class UserRegistrationContract
{
    public TelegramData TelegramData { get; set; }

    public static UserRegistrationContract Create(TelegramData data)
    {
        return new UserRegistrationContract { TelegramData = data };
    }
}