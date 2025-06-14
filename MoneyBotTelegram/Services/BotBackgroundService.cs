using MoneyBotTelegram.Commands.Common;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoneyBotTelegram.Services;

public class BotBackgroundService(
    ITelegramBotClient botClient,
    IServiceProvider serviceProvider,
    BotUpdateHandler updateHandler,
    ILogger<BotBackgroundService> logger)
    : BackgroundService
{
    private readonly ReceiverOptions _receiverOptions = new()
    {
        AllowedUpdates =
        [
            UpdateType.Message,
            UpdateType.CallbackQuery,
        ],
        DropPendingUpdates = false,
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var commandMetadatas = scope.ServiceProvider.GetRequiredService<IEnumerable<CommandMetadata>>();
            var botCommands = commandMetadatas.OrderBy(c => c.Order).Select(x => new BotCommand(x.Command, x.Description));
            await botClient.SetMyCommands(botCommands, cancellationToken: stoppingToken);
        }

        var me = await botClient.GetMe(stoppingToken);
        logger.LogInformation("Bot @{Username} started", me.Username);

        botClient.StartReceiving(updateHandler.HandleUpdateAsync, updateHandler.HandlePollingErrorAsync, _receiverOptions, stoppingToken);
    }
}
