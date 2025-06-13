using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Commands.Family;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Account;

public class StartCommandHandler(IKeyboardFactory keyboardFactory) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("/start", "Начать работу с ботом");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var text =
            $"""
             👋 Добро пожаловать! Я ваш финансовый помощник.
             🚀 Чтобы начать:
             1. Используйте {JoinCommandHandler.Metadata.Command} для регистрации
             2. Используйте {JoinFamilyCommandHandler.Metadata.Command} для добавления к семье
             2. Посмотрите {HelpCommandHandler.Metadata.Command} для списка команд
             3. Используйте {MeCommandHandler.Metadata.Command} для вашего профиля
             """;

        var keyboard = await keyboardFactory.CreateDefault(message.From.Id);

        if (editMessage)
        {
            await bot.EditMessageText(message.Chat.Id, message.Id, text, replyMarkup: keyboard);
        }
        else
        {
            await bot.SendMessage(message.Chat.Id, text, replyMarkup: keyboard);
        }
    }
}
