using Microsoft.Extensions.DependencyInjection;
using MoneyBotTelegram.Services;
using System;
using Telegram.Bot;
using Telegram.Bot.Types;
using static Telegram.Bot.TelegramBotClient;

namespace MoneyBotTelegram.Commands.Common;

public class CommandRouter
{
    private readonly List<IBotCommandHandler> _handlers;
    private readonly IBotCommandHandler _defaultHandler;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandRouter> _logger;

    public CommandRouter(IEnumerable<IBotCommandHandler> handlers, IServiceProvider serviceProvider, ILogger<CommandRouter> logger)
    {
        _handlers = handlers.ToList();

        _defaultHandler = _handlers.FirstOrDefault(x => x is DefaultCommandHandler)
                          ?? throw new InvalidOperationException("Default command handler not found");

        _serviceProvider = serviceProvider;

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

    public async Task<bool> HandleCommandAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        if((string.IsNullOrEmpty(message.Text) && message.Photo == null) || message.From == null)
            return false;

        _logger.LogInformation("Полученное сообщение от {UserId}: {Text}", message.From.Id, message.Text);

        var handler = GetHandlerWithDefault(message);

        await handler.HandleAsync(bot, message, cancellationToken, editMessage);

        bool redirected = false;

        var redirect = handler as IRedirect;
        if (redirect != null && redirect.CanRedirect(message))
        {
            redirected = await redirect.HandleRedirect(_serviceProvider, bot, message, cancellationToken);
        }
        
        if(!redirected)
        {
            var navigationState = new NavigationState()
            {
                HandlerName = handler.GetType().Name,
                CommandOrCallback = message.Text,
                IsMessage = true
            };

            await using var scope = _serviceProvider.CreateAsyncScope();
            var navigationService = scope.ServiceProvider.GetRequiredService<IUserNavigationService>();
            await navigationService.SetCurrent(message.From.Id, navigationState);
        }


        return true;
    }
}
