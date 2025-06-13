using MoneyBotTelegram.Commands;

namespace MoneyBotTelegram;

public class GlobalConstants
{
    public const char Separator = ' ';

    public const string NeedRegisterMessage = "Вначале необходимо присоединиться";

    public class Callbacks
    {
        public const string CategoryPrefix = "category";
        public const string BackPrefix = "back";
        public const string MainMenuPrefix = "main_menu";
    }
}
