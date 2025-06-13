using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries;

public class BackCallbackHandler(
    IServiceProvider serviceProvider,
    IUserNavigationService userNavigationService
    ) : BaseCallback
{
    public override string Prefix => GlobalConstants.Callbacks.BackPrefix;

    public override async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;

        var currentState = await userNavigationService.GetCurrent(userId);
        NavigationState? backState = null;

        while(backState == null)
        {
            if(currentState == null)
            {
                backState = await userNavigationService.PopAsync(userId) ?? GetDefaultUpdate(callbackQuery);
                break;
            }

            var state = await userNavigationService.PopAsync(userId) ?? GetDefaultUpdate(callbackQuery);

            if (currentState.HandlerName != state.HandlerName)
            {
                backState = state;
            }
        }

        if (backState.IsMessage)
        {
            var message = new Message
            {
                Id = callbackQuery.Message.Id,
                Chat = callbackQuery.Message.Chat,
                From = callbackQuery.From,
                Text = backState.CommandOrCallback
            };

            var commandRouter = serviceProvider.GetRequiredService<CommandRouter>();
            await commandRouter.HandleCommandAsync(bot, message, cancellationToken, true);
        }
        else
        {
            var cb = new CallbackQuery
            {
                Id = Guid.NewGuid().ToString(),
                From = callbackQuery.From,
                Data = backState.CommandOrCallback,
                Message = callbackQuery.Message
            };

            var callbackRouter = serviceProvider.GetRequiredService<CallbackQueryRouter>();
            await callbackRouter.HandleCallbackAsync(bot, cb, cancellationToken);
        }
    }

    private NavigationState GetDefaultUpdate(CallbackQuery callbackQuery)
    {
        return new NavigationState()
        {
            CommandOrCallback = StartCommandHandler.Metadata.Command,
            IsMessage = true
        };
    }
}
