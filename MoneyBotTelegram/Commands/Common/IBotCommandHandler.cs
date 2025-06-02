using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Common;

public interface IBotCommandHandler
{
    public bool CanHandle(Message message);
    Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken);
}
