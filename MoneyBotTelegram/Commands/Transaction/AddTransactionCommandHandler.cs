using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Commands.Transaction.Models;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Transaction;

public class AddTransactionCommandHandler(
    IUserService userService,
    IDraftService<TransactionInput> draft,
    IConversationState<AddTransactionState> conversation,
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<AddTransactionArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/transaction_add", "Добавить транзакцию");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From!.Id) != AddTransactionState.Idle;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var user = message.From!;
        var text = message.Text!;

        var dbUser = await userService.GetAsync(user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        var args = ParseArgs(message);
        if (args?.State != null)
        {
            conversation.SetState(user.Id, args.State.Value);
        }

        var state = conversation.GetState(user.Id);

        var responseText = string.Empty;
        var keyboard = keyboardFactory.Empty();

        switch (state)
        {
            case AddTransactionState.Idle:
                {
                    (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    break;
                }
            case AddTransactionState.EnteringAmount:
                {
                    if (decimal.TryParse(text, out var amount))
                    {
                        draft.UpdateDraft(user.Id, t => t.Amount = amount);

                        conversation.SetState(user.Id, AddTransactionState.Idle);

                        (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    }
                    else
                    {
                        await SendOrEditMessage(bot, message, "💲 Введите сумму:", null, editMessage);
                        return;
                    }
                    break;
                }
            case AddTransactionState.EnteringDescription:
                {
                    if (!text.StartsWith(Command))
                    {
                        draft.UpdateDraft(user.Id, t => t.Description = text);

                        conversation.SetState(user.Id, AddTransactionState.Idle);

                        (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    }
                    else
                    {
                        await SendOrEditMessage(bot, message, "📝 Введите описание:", null, editMessage);
                    }
                    break;
                }
            case AddTransactionState.EnteringCategory:
                {
                    if (args.CategoryId.HasValue)
                    {
                        draft.UpdateDraft(user.Id, t => t.CategoryId = args.CategoryId.Value);

                        conversation.SetState(user.Id, AddTransactionState.Idle);

                        (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    }
                    else
                    {
                        var categories = await db.Categories.ToListAsync();

                        keyboard.AddPaginated(
                            categories,
                            (i) => i.Name,
                            (i) => BuildArgs(new() { CategoryId = i.Id }),
                            (p) => BuildArgs(new() { Page = p }),
                            args!.Page ?? 1,
                            5);

                        responseText += "📋 Выберите категорию:";
                    }
                    break;
                }
            case AddTransactionState.EnteringDate:
                {
                    if (DateOnly.TryParse(text, out var date))
                    {
                        draft.UpdateDraft(user.Id, t => t.Date = date);

                        conversation.SetState(user.Id, AddTransactionState.Idle);

                        (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    }
                    else
                    {
                        await SendOrEditMessage(bot, message, "📅 Введите дату формата dd.MM.yyyy:", null, editMessage);
                        return;
                    }
                    break;
                }
            case AddTransactionState.EnteringPlace:
                {
                    if (args.PlaceId.HasValue)
                    {
                        draft.UpdateDraft(user.Id, t => t.PlaceId = args.PlaceId.Value);

                        conversation.SetState(user.Id, AddTransactionState.Idle);

                        (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    }
                    else
                    {
                        var places = await db.Places.ToListAsync();

                        keyboard.AddPaginated(
                            places,
                            (i) => i.Name,
                            (i) => BuildArgs(new() { PlaceId = i.Id }),
                            (p) => BuildArgs(new() { Page = p }),
                            args!.Page ?? 1,
                            5);

                        responseText += "🏦 Выберите место:";
                    }
                    break;
                }
            case AddTransactionState.Clear:
                {
                    draft.ClearDraft(user.Id);
                    conversation.SetState(user.Id, AddTransactionState.Idle);

                    (responseText, keyboard) = GetBase(draft.GetDraft(user.Id));
                    break;
                }
            case AddTransactionState.Save:
                {
                    conversation.SetState(user.Id, AddTransactionState.Idle);

                    var transactionInput = draft.GetDraft(user.Id);

                    if (!transactionInput.Amount.HasValue)
                    {
                        responseText += "❌ Ошибка сохранения. Сумма не заполнена\n\n";
                    }
                    else if (!transactionInput.CategoryId.HasValue)
                    {
                        responseText += "❌ Ошибка сохранения. Категория не выбрана\n\n";
                    }
                    else
                    {
                        try
                        {
                            var transaction = new MoneyTransaction()
                            {
                                Amount = transactionInput.Amount.Value,
                                Description = transactionInput.Description,
                                CategoryId = transactionInput.CategoryId.Value,
                                PlaceId = transactionInput.PlaceId,
                                Date = transactionInput.Date,
                                UserId = user.Id,
                            };
                            await db.MoneyTransactions.AddAsync(transaction);
                            await db.SaveChangesAsync();

                            draft.ClearDraft(user.Id);

                            responseText += "✅ Транзакция успешно добавлена\n\n";
                        }
                        catch (Exception ex)
                        {
                            responseText += "❌ Ошибка сохранения\n\n";
                            logger.LogError(ex, "Ошибка сохранения транзакции");
                        }
                    }

                    responseText += GetStatus(draft.GetDraft(user.Id));
                    keyboard = GetBaseKeyboard();

                    break;
                }
        }

        await SendOrEditMessage(bot, message, responseText, keyboard.Create(), editMessage);
    }

    private (string, IKeyboardFactory) GetBase(TransactionInput draft)
    {
        return (GetStatus(draft), GetBaseKeyboard());
    }

    private string GetStatus(TransactionInput draft)
    {
        var category = db.Categories.FirstOrDefault(c => c.Id == draft.CategoryId);
        var place = db.Places.FirstOrDefault(c => c.Id == draft.PlaceId);
        return $"""
            💲 Сумма: {draft.Amount}
            📝 Описание: {draft.Description}
            📋 Категория: {(category == null ? "не выбрана" : category.Name)}
            📅 Дата: {draft.Date:dd.MM.yyyy}
            🏪 Место: {(place == null ? "не выбрано" : place.Name)}
            """;
    }

    private IKeyboardFactory GetBaseKeyboard()
    {
        var keyboard = keyboardFactory.Empty();

        keyboard.AddButton("💲 Ввести сумму", BuildArgs(new() { State = AddTransactionState.EnteringAmount })).AddNewLine();
        keyboard.AddButton("📝 Ввести описание", BuildArgs(new() { State = AddTransactionState.EnteringDescription })).AddNewLine();
        keyboard.AddButton("📋 Выбрать категорию", BuildArgs(new() { State = AddTransactionState.EnteringCategory })).AddNewLine();
        keyboard.AddButton("📅 Выбрать дату", BuildArgs(new() { State = AddTransactionState.EnteringDate })).AddNewLine();
        keyboard.AddButton("🏦 Выбрать место", BuildArgs(new() { State = AddTransactionState.EnteringPlace })).AddNewLine();
        keyboard.AddButton("🗑 Очистить", BuildArgs(new() { State = AddTransactionState.Clear }))
            .AddButton("✅ Добавить", BuildArgs(new() { State = AddTransactionState.Save })).AddNewLine();
        keyboard.AddBackButton().AddNewLine();

        return keyboard;
    }
}
