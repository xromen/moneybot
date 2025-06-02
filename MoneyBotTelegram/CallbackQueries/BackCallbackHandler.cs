using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries;

public class BackCallbackHandler(
    IUserService userService,
    IServiceProvider serviceProvider,
    IUserNavigationService userNavigationService,
    BotUpdateHandler updateHandler
    ) : BaseCallback
{
    public override string Prefix => "back";

    public override async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        var userId = callbackQuery.From.Id;

        var update = await userNavigationService.PopAsync(userId) ?? GetDefaultUpdate(callbackQuery);

        await updateHandler.HandleUpdateAsync(bot, update, cancellationToken);
    }

    private Update GetDefaultUpdate(CallbackQuery callbackQuery)
    {
        return new Update()
        {
            Message = new Message()
            {
                Chat = callbackQuery.Message.Chat,
                Text = StartCommandHandler.Metadata.Command,
                From = callbackQuery.From
            }
        };
    }
}
