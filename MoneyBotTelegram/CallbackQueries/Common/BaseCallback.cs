using MoneyBotTelegram.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries.Common;

public abstract class BaseCallback : ICallbackQueryHandler
{
    public abstract string Prefix { get; }
    public bool CanHandle(CallbackQuery callback)
    {
        return callback.Data!.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase);
    }

    public abstract Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken);
}

public abstract class BaseCallback<TArgs> : BaseCallback where TArgs : new()
{
    public TArgs ParseArgs(CallbackQuery callback)
    {
        return ArgsParser<TArgs>.ParseArgs(callback.Data!, Prefix);
    }

    public string BuildArgs(TArgs args)
    {
        return ArgsParser<TArgs>.BuildArgs(args, Prefix);
    }
}
