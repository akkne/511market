namespace ResaleTelegramBot.Telegram.Texts.Input;

public class RegularTextStaticTexts
{
    public static string MainMenu = """
                                    Главное меню
                                    """;

    public static string FindListings = """
                                        🛍 Найти товары
                                        """;

    public static string FindByText = """
                                      🔍 Поиск по тексту
                                      """;

    public static string Categories = """
                                      📂 Категории
                                      """;

    public static string ChooseCity = """
                                      🏙 Выбор города (фильтр)
                                      """;

    public static string AddListing = """
                                      ➕ Разместить объявление
                                      """;

    public static string Favorite = """
                                    ⭐ Избранное
                                    """;

    public static string MyProfile = """
                                     👤 Мой профиль
                                     """;

    public static string Settings = """
                                    ⚙️ Настройки
                                    """;

    public static string[] All =
    [
        MainMenu,
        FindListings,
        FindByText,
        Categories,
        ChooseCity,
        AddListing,
        Favorite,
        MyProfile,
        Settings
    ];
}