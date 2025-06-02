using MoneyBotTelegram.Commands.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands;

public class HelpCommandHandler : BaseCommand, ICommandMetadata
{
    private readonly IEnumerable<CommandMetadata> _commands;

    public HelpCommandHandler(IEnumerable<CommandMetadata> commands)
    {
        _commands = commands
            .Where(x => string.IsNullOrEmpty(x.Description) == false)
            .OrderBy(x => x.Command);
    }

    public static CommandMetadata Metadata { get; } = new("/help", "Показать справку");

    public override string Command => Metadata.Command;

    public override Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        var helpText = string.Join("\n", _commands.Select(c => $"{c.Command} - {c.Description}"));
        return bot.SendMessage(message.Chat.Id, helpText);
    }
}
