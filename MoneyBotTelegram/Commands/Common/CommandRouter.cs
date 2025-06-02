using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Common;

public class CommandRouter
{
    private readonly List<IBotCommandHandler> _handlers;
    private readonly IBotCommandHandler _defaultHandler;
    private readonly ILogger<CommandRouter> _logger;

    public CommandRouter(IEnumerable<IBotCommandHandler> handlers, ILogger<CommandRouter> logger)
    {
        _handlers = handlers.ToList();

        _defaultHandler = _handlers.FirstOrDefault(x => x is DefaultCommandHandler)
                          ?? throw new InvalidOperationException("Default command handler not found");

        _logger = logger;
    }

    public IBotCommandHandler? GetHandler(Message message)
    {
        return _handlers.FirstOrDefault(c => c.CanHandle(message));
    }

    public IBotCommandHandler GetHandlerWithDefault(Message message)
    {
        var handler = GetHandler(message);
        handler ??= _defaultHandler;
        return handler;
    }

    public Task HandleCommandAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(message.Text) || message.From == null)
            return Task.CompletedTask;

        _logger.LogInformation("Полученное сообщение от {UserId}: {Text}", message.From.Id, message.Text);

        var handle = GetHandlerWithDefault(message);

        return handle.HandleAsync(bot, message, cancellationToken);
    }
}
