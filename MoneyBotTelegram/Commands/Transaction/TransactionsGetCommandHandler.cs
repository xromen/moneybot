using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Commands.Transaction.Models;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot.Types;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Globalization;
using System.ComponentModel;

namespace MoneyBotTelegram.Commands.Transaction;

public class TransactionsGetArgs
{
    public DatePeriod? Period { get; set; }
    public DateOnly? DateFrom {  get; set; }
    public DateOnly? DateTo { get; set; }
}

public enum DatePeriod
{
    [Description("День")]
    Day,

    [Description("Неделя")]
    Week,

    [Description("Месяц")]
    Month,

    [Description("Год")]
    Year,

    [Description("Свой")]
    Custom
}

public static class DatePeriodExtensions
{
    public static string GetDescription(this DatePeriod? period)
    {
        var fi = period.GetType().GetField(period.ToString());
        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (attributes.Length > 0)
            return attributes[0].Description;
        else
            return period.ToString();
    }
}

public enum TransactionsGetState
{
    Idle, 
    EnteringPeriod
}

public class TransactionsGetCommandHandler(
    IUserService userService,
    ApplicationDbContext db,
    IConversationState<TransactionsGetState> conversation,
    IDraftService<TransactionsGetArgs> argsDraft,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<TransactionsGetArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/transactions_get", "Вывести список транзакций");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From!.Id) != TransactionsGetState.Idle;
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

        if (args != null && !args.Period.HasValue)
        {
            args = argsDraft.GetDraft(user.Id);
            args.Period ??= DatePeriod.Day;
        }

        var familyMembers = await db.Users.Where(c => c.Id == user.Id || c.FamilyParentId == user.Id || c.Id == dbUser.FamilyParentId || c.FamilyParentId == dbUser.FamilyParentId).ToListAsync();
        var familyIds = familyMembers.Select(c => c.Id).ToList();

        var today = DateTime.Now;
        var dateFrom = DateOnly.FromDateTime(today);
        var dateTo = DateOnly.FromDateTime(today);

        var state = conversation.GetState(user.Id);

        if(state == TransactionsGetState.EnteringPeriod)
        {
            var splitted = text.Split('-');

            if(splitted.Length != 2 || !DateOnly.TryParse(splitted[0], out dateFrom) || !DateOnly.TryParse(splitted[1], out dateTo))
            {
                await bot.SendMessage(message.Chat.Id, "❌ Ошибка парсинга периода. Введите период еще раз");
                return;
            }

            args = new()
            {
                Period = DatePeriod.Custom,
                DateFrom = dateFrom,
                DateTo = dateTo,
            };

            conversation.SetState(user.Id, TransactionsGetState.Idle);
        }

        argsDraft.UpdateDraft(user.Id, a => a.Period = args.Period);
        argsDraft.UpdateDraft(user.Id, a => a.DateFrom = args.DateFrom);
        argsDraft.UpdateDraft(user.Id, a => a.DateTo = args.DateTo);


        switch (args.Period)
        {
            case DatePeriod.Day:
                {
                    dateFrom = DateOnly.FromDateTime(DateTime.Now);
                    dateTo = DateOnly.FromDateTime(DateTime.Now);
                    break;
                }
            case DatePeriod.Week:
                {
                    int delta = DayOfWeek.Monday - today.DayOfWeek;

                    // Если сегодня воскресенье (DayOfWeek == 0), нужно привести к -6
                    if (delta > 0)
                        delta -= 7;

                    dateFrom = DateOnly.FromDateTime(today.AddDays(delta));
                    dateTo = dateFrom.AddDays(6);

                    break;
                }
            case DatePeriod.Month:
                {
                    dateFrom = new DateOnly(today.Year, today.Month, 1);
                    dateTo = dateFrom.AddMonths(1).AddDays(-1);
                    break;
                }
            case DatePeriod.Year:
                {
                    dateFrom = new DateOnly(today.Year, 1, 1);
                    dateTo = new DateOnly(today.Year, 12, 31);
                    break;
                }
            case DatePeriod.Custom:
                {
                    if(args.DateFrom.HasValue && args.DateTo.HasValue)
                    {
                        dateFrom = args.DateFrom.Value;
                        dateTo = args.DateTo.Value;
                        break;
                    }

                    conversation.SetState(user.Id, TransactionsGetState.EnteringPeriod);
                    await SendOrEditMessage(bot, message, "📅 Введите период в формате dd.MM.yyyy-dd.MM.yyyy", null, editMessage);
                    return;
                }
        }
        
        StringBuilder responseBuilder = new StringBuilder($"📈 Ваши доходы и траты за период ({args.Period.GetDescription()}) {dateFrom:dd.MM.yyyy} - {dateTo:dd.MM.yyyy}\n");
        responseBuilder.AppendLine();

        var keyboard = keyboardFactory.Empty();

        var transactions = await db.MoneyTransactions
            .Include(c => c.User)
            .Include(c => c.Category)
            .Include(c => c.Place)
            .Where(c => familyIds.Contains(c.UserId) && dateFrom <= c.Date && c.Date <= dateTo)
            .OrderBy(c => c.Date)
            .ThenBy(c => c.Id)
            .ToListAsync();

        if (!transactions.Any())
        {
            responseBuilder.AppendLine("За выбранный период транзакций нет");
        }
        else
        {
            var periodAmount = transactions.Sum(c => c.Amount);
            responseBuilder.AppendLine($"📊 Итог за период {periodAmount.ToString("C", new CultureInfo("ru-RU"))}\n");
            foreach(var day in transactions.Select(c => c.Date).Distinct())
            {
                var dayTransactions = transactions.Where(c => c.Date == day);
                var dayAmount = dayTransactions.Sum(c => c.Amount);
                responseBuilder.AppendLine($"{day.ToString("dd.MM.yyyy")} ({day.ToString("ddd")}):");
                responseBuilder.AppendLine($"📊 Итог дня {dayAmount.ToString("C", new CultureInfo("ru-RU"))}");

                foreach(var transactionUser in dayTransactions.Select(c => c.User).DistinctBy(c => c.Id))
                {
                    responseBuilder.AppendLine($"    {transactionUser.FirstName}:");

                    foreach(var transaction in dayTransactions.Where(c => c.UserId == transactionUser.Id))
                    {
                        var transactionType = string.Empty;

                        if(transaction.Amount >= 0)
                        {
                            transactionType = "+";
                        }
                        else
                        {
                            transactionType = "-";
                        }

                        responseBuilder.AppendLine($"        {transactionType} {Math.Abs(transaction.Amount).ToString("C", new CultureInfo("ru-RU"))} {transaction.Description ?? transaction.Category.Name} {transaction.Place?.Name}");
                    }
                }

                responseBuilder.AppendLine();
            }
        }

        keyboard.AddButton("📅 День", BuildArgs(new() { Period = DatePeriod.Day}));
        keyboard.AddButton("📅 Неделя", BuildArgs(new() { Period = DatePeriod.Week})).AddNewLine();
        keyboard.AddButton("📅 Месяц", BuildArgs(new() { Period = DatePeriod.Month}));
        keyboard.AddButton("📅 Год", BuildArgs(new() { Period = DatePeriod.Year})).AddNewLine();
        keyboard.AddButton("⚙️ Выбрать период", BuildArgs(new() { Period = DatePeriod.Custom})).AddNewLine();

        keyboard.AddBackButton();

        await SendOrEditMessage(bot, message, responseBuilder.ToString(), keyboard.Create(), editMessage);
    }
}
