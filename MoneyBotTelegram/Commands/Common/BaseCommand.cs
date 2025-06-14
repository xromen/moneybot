using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Common;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace MoneyBotTelegram.Commands.Common;

public abstract class BaseCommand : IBotCommandHandler
{
    public abstract string Command { get; }
    public virtual bool CanHandle(Message message)
    {
        return !string.IsNullOrEmpty(message.Text) && message.Text!.StartsWith(Command, StringComparison.OrdinalIgnoreCase);
    }

    public abstract Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false);

    public async Task SendOrEditMessage(ITelegramBotClient bot, Message requestMessage, string text, InlineKeyboardMarkup? markup, bool editMessage = false)
    {
        if (editMessage)
        {
            await bot.EditMessageText(requestMessage.Chat.Id, requestMessage.Id, text, replyMarkup: markup);
        }
        else
        {
            await bot.SendMessage(requestMessage.Chat.Id, text, replyMarkup: markup);
        }
    }
}

public interface IRedirect
{
    public bool CanRedirect(Message message);

    public Task<bool> HandleRedirect(
        IServiceProvider serviceProvider,
        ITelegramBotClient bot,
        Message message,
        CancellationToken cancellationToken);
}

public abstract class BaseCommand<TArgs> : BaseCommand, IRedirect where TArgs : BaseArgs
{
    private static Dictionary<long, string> redirects = new Dictionary<long, string>();

    protected void SaveRedicrect(long userId, string data)
    {
        redirects[userId] = data;
    }

    protected string PopRedirect(long userId)
    {
        if (redirects.ContainsKey(userId))
        {
            var data = redirects[userId];
            redirects.Remove(userId);

            return data;
        }

        return string.Empty;
    }

    public TArgs? ParseArgs(Message message)
    {
        return ArgsParser<TArgs>.ParseArgs(message.Text!, Command);
    }

    public string BuildArgs(TArgs args)
    {
        return ArgsParser<TArgs>.BuildArgs(args, Command);
    }

    public virtual bool CanRedirect(Message message)
    {
        return true;
    }

    public virtual async Task<bool> HandleRedirect(
        IServiceProvider serviceProvider,
        ITelegramBotClient bot, 
        Message message, 
        CancellationToken cancellationToken)
    {
        var args = ParseArgs(message);

        if (args == null || string.IsNullOrEmpty(args.Redirect))
        {
            return false;
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        var commandRouter = scope.ServiceProvider.GetRequiredService<CommandRouter>();

        message.Text = args.Redirect;

        return await commandRouter.HandleCommandAsync(bot, message, cancellationToken, false);
    }
}
