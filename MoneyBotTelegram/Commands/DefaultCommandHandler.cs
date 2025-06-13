using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands;

public class DefaultCommandHandler(IUserService userService) : BaseCommand
{
    public override string Command => string.Empty;

    public override bool CanHandle(Message message)
    {
        return false;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var userId = message.From?.Id;

        var exists = await userService.ExistsAsync(userId.Value, cancellationToken);

        var text = exists
            ? $"Не понимаю команду. Используйте {HelpCommandHandler.Metadata.Command} для списка команд"
            : $"Для начала работы выполните {JoinCommandHandler.Metadata.Command}";

        await bot.SendMessage(message.Chat.Id, text);
    }
}
