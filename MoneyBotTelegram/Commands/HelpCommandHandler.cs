using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands;

public class HelpCommandHandler : BaseCommand, ICommandMetadata
{
    private readonly IEnumerable<CommandMetadata> _commands;
    private readonly IKeyboardFactory _keyboardFactory;

    public HelpCommandHandler(IEnumerable<CommandMetadata> commands, IKeyboardFactory keyboardFactory)
    {
        _commands = commands
            .Where(x => string.IsNullOrEmpty(x.Description) == false)
            .OrderBy(x => x.Command);

        _keyboardFactory = keyboardFactory;
    }

    public static CommandMetadata Metadata { get; } = new("/help", "Показать справку");

    public override string Command => Metadata.Command;

    public override Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var helpText = string.Join("\n", _commands.Select(c => $"{c.Command} - {c.Description}"));
        var keyboard = _keyboardFactory.AddBackButton();

        if (editMessage)
        {
            return bot.EditMessageText(message.Chat.Id, message.Id, helpText, replyMarkup: keyboard.Create());
        }
        else
        {
            return bot.SendMessage(message.Chat.Id, helpText, replyMarkup: keyboard.Create());
        }
    }
}
