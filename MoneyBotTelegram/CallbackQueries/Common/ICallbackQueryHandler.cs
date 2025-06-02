using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries.Common;

public interface ICallbackQueryHandler
{
    bool CanHandle(CallbackQuery callback);
    Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken);
}
