using MoneyBotTelegram.CallbackQueries;
using MoneyBotTelegram.Commands;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Transaction;
using MoneyBotTelegram.Infrasctructure.Entities;
using Telegram.Bot.Types.ReplyMarkups;

namespace MoneyBotTelegram.Services;

public interface IKeyboardFactory
{
    IKeyboardFactory AddButton(string text, string command);
    IKeyboardFactory AddNewLine();
    IKeyboardFactory AddBackButton();
    IKeyboardFactory AddToMainMenuButton();
    IKeyboardFactory Empty();
    bool IsEmpty();
    InlineKeyboardMarkup Create();
    Task<InlineKeyboardMarkup> CreateDefault(long? userId = null, CancellationToken cancellationToken = default);
    IKeyboardFactory AddPaginated<T>(
        IEnumerable<T> items,
        Func<T, string> itemFormatter,
        Func<T, string> callbackData,
        Func<int, string> paginateData,
        int page,
        int pageSize) where T : BaseEntity;
}
public class KeyboardFactory(IUserService userService) : IKeyboardFactory
{
    private List<InlineKeyboardButton[]> _buttons = new();
    private List<InlineKeyboardButton> _lineButtons = new();

    public IKeyboardFactory AddButton(string text, string command)
    {
        if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(command))
        {
            _lineButtons.Add(InlineKeyboardButton.WithCallbackData(text, command));
        }
        return this;
    }
    public IKeyboardFactory AddNewLine()
    {
        if (_lineButtons.Count > 0)
        {
            _buttons.Add(_lineButtons.ToArray());
            _lineButtons.Clear();
        }
        return this;
    }
    public IKeyboardFactory AddBackButton()
    {
        return AddButton("🔙 Назад", GlobalConstants.Callbacks.BackPrefix);
    }
    public IKeyboardFactory AddToMainMenuButton()
    {
        return AddButton("🏠 Домой", StartCommandHandler.Metadata.Command);
    }
    public IKeyboardFactory Empty()
    {
        _buttons.Clear();
        _lineButtons.Clear();
        return this;
    }
    public bool IsEmpty()
    {
        return _buttons.Count == 0 && _lineButtons.Count == 0;
    }
    public InlineKeyboardMarkup Create()
    {
        AddNewLine();
        return new(_buttons);
    }

    public IKeyboardFactory AddPaginated<T>(
        IEnumerable<T> items,
        Func<T, string> itemFormatter,
        Func<T, string> callbackData,
        Func<int, string> paginateData,
        int page,
        int pageSize) where T : BaseEntity
    {
        var pagedItems = items.Skip((page - 1) * pageSize).Take(pageSize);

        foreach(var item in pagedItems)
        {
            var text = itemFormatter(item);
            var callback = callbackData(item);
            AddButton(text, callback).AddNewLine();
        }

        var totalPages = (int)Math.Ceiling(items.Count() / (double)pageSize);
        var navigationButtons = new List<InlineKeyboardButton>();

        if (page > 1)
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️", paginateData(page - 1)));

        navigationButtons.Add(InlineKeyboardButton.WithCallbackData($"{page}/{totalPages}", $"asdasd_info"));

        if (page < totalPages)
            navigationButtons.Add(InlineKeyboardButton.WithCallbackData("➡️", paginateData(page + 1)));

        _buttons.Add(navigationButtons.ToArray());

        return this;
    }

    public async Task<InlineKeyboardMarkup> CreateDefault(long? userId = null, CancellationToken cancellationToken = default)
    {
        List<InlineKeyboardButton[]> buttons = [];
        var exists = userId != null && await userService.ExistsAsync(userId.Value, cancellationToken);

        if (exists)
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData("🏪 Места", "PlaceCallbackHandler.MakeCallbackData()"),
                InlineKeyboardButton.WithCallbackData("📋 Категории транзакций", GlobalConstants.Callbacks.CategoryPrefix),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("📌 Мой профиль", MeCommandHandler.Metadata.Command),
                InlineKeyboardButton.WithCallbackData("❓ Помощь", HelpCommandHandler.Metadata.Command),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("📄 Список транзакций", TransactionsGetCommandHandler.Metadata.Command),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("➕ Транзакция", AddTransactionCommandHandler.Metadata.Command),
            ]);
        }
        else
        {
            buttons.Add([
                InlineKeyboardButton.WithCallbackData("🎯 Присоединиться", JoinCommandHandler.Metadata.Command),
            ]);

            buttons.Add([
                InlineKeyboardButton.WithCallbackData("❓ Помощь", HelpCommandHandler.Metadata.Command),
            ]);
        }

        return new(buttons);
    }
}
