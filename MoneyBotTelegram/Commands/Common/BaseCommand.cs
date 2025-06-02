using MoneyBotTelegram.Common;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Common;

public abstract class BaseCommand : IBotCommandHandler
{
    public abstract string Command { get; }
    public virtual bool CanHandle(Message message)
    {
        return message.Text!.StartsWith(Command, StringComparison.OrdinalIgnoreCase);
    }

    public abstract Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken);
}

public abstract class BaseCommand<TArgs> : BaseCommand where TArgs : new()
{
    public TArgs ParseArgs(Message message)
    {
        return ArgsParser<TArgs>.ParseArgs(message.Text!, Command);
    }

    public string BuildArgs(TArgs args)
    {
        return ArgsParser<TArgs>.BuildArgs(args, Command);
    }
}
