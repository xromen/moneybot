using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MoneyBotTelegram;

public class BotUpdateHandler(
    ILogger<BotUpdateHandler> logger,
    IServiceProvider serviceProvider)
{
    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var commandRouter = scope.ServiceProvider.GetRequiredService<CommandRouter>();
            var callbackRouter = scope.ServiceProvider.GetRequiredService<CallbackQueryRouter>();

            bool handled = false;

            switch (update.Type)
            {
                case UpdateType.Message:
                    handled = await commandRouter.HandleCommandAsync(bot, update.Message!, cancellationToken);
                    break;
                case UpdateType.CallbackQuery:
                    handled = await callbackRouter.HandleCallbackAsync(bot, update.CallbackQuery!, cancellationToken);
                    break;
                default:
                    await HandleUnknownUpdateAsync(update);
                    return;
            };
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Обработка ошибок Update {UpdateId}", update.Id);
        }
    }

    private Task HandleUnknownUpdateAsync(Update update)
    {
        logger.LogWarning("Получен неподдерживаемый тип обновления: {UpdateType}", update.Type);
        return Task.CompletedTask;
    }

    //private async Task HandleMessageAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    //{
    //    if (message.From == null || message.Text == null)
    //    {
    //        return;
    //    }

    //    var userId = message.From.Id;
    //    var text = message.Text;

    //    logger.LogInformation("Полученное сообщение от {UserId}: {Text}", userId, text);

    //    await using var scope = serviceProvider.CreateAsyncScope();

    //    var handler = scope.ServiceProvider.GetRequiredService<CommandRouter>().GetHandlerWithDefault(message.Text, userId);
    //    var response = await handler.HandleAsync(message, cancellationToken);

    //    if (!string.IsNullOrWhiteSpace(response.Message))
    //    {
    //        await EditOrSendResponse(bot, message.Chat.Id, null, response, cancellationToken);
    //    }

    //    //if (!string.IsNullOrWhiteSpace(response.RedirectCallbackData) || !string.IsNullOrWhiteSpace(response.RedirectCommand))
    //    //{
    //    //    CallbackQuery fake = new()
    //    //    {
    //    //        Id = Guid.NewGuid().ToString(),
    //    //        Data = response.RedirectCallbackData ?? response.RedirectCommand,
    //    //        From = message.From,
    //    //        Message = message,
    //    //    };
    //    //    await HandleCallbackQueryAsync(bot, fake, cancellationToken, true);

    //    //    return;
    //    //}

    //    if (response.SaveInHistory && text.Contains(handler.Command))
    //    {
    //        var userHistory = scope.ServiceProvider.GetRequiredService<IUserNavigationService>();
    //        await userHistory.SetCurrent(userId, message.Text);
    //    }
    //}

    //private async Task HandleCallbackQueryAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken, bool redirectedFromMessage = false)
    //{
    //    if (callbackQuery.Data == null || callbackQuery.Message == null)
    //    {
    //        return;
    //    }

    //    await using var scope = serviceProvider.CreateAsyncScope();
    //    var userHistory = scope.ServiceProvider.GetRequiredService<IUserNavigationService>();

    //    BotResponse? response = null;

    //    var handler = scope.ServiceProvider.GetRequiredService<CallbackQueryRouter>().GetHandler(callbackQuery.Data);

    //    if (handler != null)
    //    {
    //        response = await handler.HandleCallbackAsync(callbackQuery, cancellationToken);

    //        if (!string.IsNullOrEmpty(response.RedirectCallbackData) || !string.IsNullOrEmpty(response.RedirectCommand))
    //        {
    //            callbackQuery.Data = response.RedirectCallbackData ?? response.RedirectCommand;
    //            await HandleCallbackQueryAsync(bot, callbackQuery, cancellationToken);
    //            return;
    //        }

    //        if (string.IsNullOrEmpty(response.Message))
    //        {
    //            await UpdateDynamicMarkup(bot, response.KeyboardMarkup, callbackQuery.Message, cancellationToken);
    //        }
    //        else
    //        {
    //            await EditOrSendResponse(bot,
    //                callbackQuery.Message.Chat.Id,
    //                redirectedFromMessage ? null : callbackQuery.Message.MessageId,
    //                response,
    //                cancellationToken);
    //        }
    //    }
    //    else
    //    {
    //        var commandHandler = scope.ServiceProvider.GetRequiredService<CommandRouter>().GetHandler('/' + callbackQuery.Data, callbackQuery.From.Id);

    //        if (commandHandler != null)
    //        {
    //            response = await commandHandler.HandleAsync(new() { From = callbackQuery.From, Text = callbackQuery.Data }, cancellationToken);

    //            if (!string.IsNullOrEmpty(response.RedirectCallbackData) || !string.IsNullOrEmpty(response.RedirectCommand))
    //            {
    //                callbackQuery.Data = response.RedirectCallbackData ?? response.RedirectCommand;
    //                await HandleCallbackQueryAsync(bot, callbackQuery, cancellationToken);
    //                return;
    //            }

    //            if (string.IsNullOrWhiteSpace(response.Message) == false)
    //            {
    //                await EditOrSendResponse(bot,
    //                    callbackQuery.Message.Chat.Id,
    //                    redirectedFromMessage ? null : callbackQuery.Message.MessageId,
    //                    response,
    //                    cancellationToken);
    //            }
    //        }
    //        else
    //        {
    //            logger.LogWarning("No handler found for callback: {CallbackData}", callbackQuery.Data);
    //        }
    //    }

    //    if (callbackQuery.Data != BackCallbackHandler.BackPrefix && response != null && response.SaveInHistory)
    //    {
    //        await userHistory.SetCurrent(callbackQuery.From.Id, callbackQuery.Data);
    //    }

    //    await bot.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: cancellationToken);
    //}

    //private async Task UpdateDynamicMarkup(ITelegramBotClient bot, InlineKeyboardMarkup newMarkup, Message message, CancellationToken cancellationToken)
    //{
    //    await using var scope = serviceProvider.CreateAsyncScope();
    //    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    //    var keyboardFactory = scope.ServiceProvider.GetRequiredService<IKeyboardFactory>();

    //    var user = await userService.GetAsync(message.Chat.Id, cancellationToken);

    //    await bot.EditMessageReplyMarkup(message.Chat.Id,
    //    message.MessageId,
    //        newMarkup,
    //        cancellationToken: cancellationToken);
    //}

    //private async Task EditOrSendResponse(ITelegramBotClient bot, long chatId, int? messageId, BotResponse response, CancellationToken cancellationToken)
    //{
    //    await using var scope = serviceProvider.CreateAsyncScope();
    //    var keyboardFactory = scope.ServiceProvider.GetRequiredService<IKeyboardFactory>();

    //    var keyboard = response.KeyboardMarkup;

    //    if (stateStore.GetUserState(chatId)?.UserState != UserState.WaitingArgument && !response.IsRedirect)
    //    {
    //        keyboard ??= await keyboardFactory.CreateDefault(chatId, cancellationToken);
    //    }

    //    if (messageId.HasValue)
    //    {
    //        try
    //        {
    //            await bot.EditMessageText(chatId,
    //                messageId.Value,
    //                response.Message,
    //                replyMarkup: keyboard,
    //                cancellationToken: cancellationToken);
    //        }
    //        catch (ApiRequestException exception) when (exception.Message.Contains("message is not modified"))
    //        {
    //        }
    //    }
    //    else
    //    {
    //        await bot.SendMessage(chatId,
    //            response.Message,
    //            replyMarkup: keyboard,
    //            cancellationToken: cancellationToken);
    //    }
    //}

    public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ApiRequestException api:
                logger.LogError(api, "Telegram API Error: [{ErrorCode}] {Message}", api.ErrorCode, api.Message);
                break;

            default:
                logger.LogError(exception, "Internal Error: ");
                break;
        }

        return Task.CompletedTask;
    }
}
