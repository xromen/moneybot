using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries.Common;

public class CallbackQueryRouter(
    IEnumerable<ICallbackQueryHandler> callbackHandlers, 
    IEnumerable<IBotCommandHandler> commandHandlers,
    IServiceProvider serviceProvider)
{
    //public ICallbackQueryHandler? GetHandler(string callbackData)
    //{
    //    return handlers.FirstOrDefault(h => h.CanHandle(callbackData));
    //}

    public async Task<bool> HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(callback.Data))
        {
            return false;
        }

        await using var scope = serviceProvider.CreateAsyncScope();

        if (callbackHandlers.FirstOrDefault(h => h.CanHandle(callback)) is var callbackHandler && callbackHandler != null)
        {
            await callbackHandler.HandleCallbackAsync(bot, callback, cancellationToken);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            var navigationState = new NavigationState()
            {
                HandlerName = callbackHandler.GetType().Name,
                CommandOrCallback = callback.Data,
                IsMessage = false
            };

            var navigationService = scope.ServiceProvider.GetRequiredService<IUserNavigationService>();
            await navigationService.SetCurrent(callback.From.Id, navigationState);

            return true;
        }

        var message = new Message()
        {
            Id = callback.Message.Id,
            From = callback.From,
            Text = callback.Data,
            Chat = callback.Message.Chat
        };

        if (commandHandlers.FirstOrDefault(h => h.CanHandle(message)) is var messageHandler && messageHandler != null)
        {
            await messageHandler.HandleAsync(bot, message, cancellationToken, true);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);

            bool redirected = false;

            var redirect = messageHandler as IRedirect;
            if (redirect != null && redirect.CanRedirect(message))
            {
                redirected = await redirect.HandleRedirect(serviceProvider, bot, message, cancellationToken);
            }
            
            if(!redirected)
            {
                var navigationState = new NavigationState()
                {
                    HandlerName = messageHandler.GetType().Name,
                    CommandOrCallback = callback.Data,
                    IsMessage = false
                };

                var navigationService = scope.ServiceProvider.GetRequiredService<IUserNavigationService>();
                await navigationService.SetCurrent(callback.From.Id, navigationState);
            }


            return true;
        }

        return false;
    }
}
