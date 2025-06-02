using MoneyBotTelegram.Commands.Common;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries.Common;

public class CallbackQueryRouter(
    IEnumerable<ICallbackQueryHandler> callbackHandlers, 
    IEnumerable<IBotCommandHandler> commandHandlers)
{
    //public ICallbackQueryHandler? GetHandler(string callbackData)
    //{
    //    return handlers.FirstOrDefault(h => h.CanHandle(callbackData));
    //}

    public async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(callback.Data))
        {
            return;
        }

        if(callbackHandlers.FirstOrDefault(h => h.CanHandle(callback)) is var callbackHandler && callbackHandler != null)
        {
            await callbackHandler.HandleCallbackAsync(bot, callback, cancellationToken);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);
            return;
        }

        var message = new Message()
        {
            From = callback.From,
            Text = callback.Data,
            Chat = callback.Message.Chat
        };

        if (commandHandlers.FirstOrDefault(h => h.CanHandle(message)) is var messageHandler && messageHandler != null)
        {
            await messageHandler.HandleAsync(bot, message, cancellationToken);
            await bot.AnswerCallbackQuery(callback.Id, cancellationToken: cancellationToken);
            return;
        }
    }
}
