using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.CallbackQueries;

public class CategoryArgs
{
    public long? Id { get; set; }
    public int? Nav { get; set; }
    public bool? Add { get; set; }
}

public class CategoryCallbackHandler(
    IEntityCacheService<Category> categoryCache,
    IKeyboardFactory keyboardFactory,
    IServiceProvider serviceProvider) : BaseCallback<CategoryArgs>
{
    public override string Prefix => GlobalConstants.Callbacks.CategoryPrefix;

    public override async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        //await using var scope = serviceProvider.CreateAsyncScope();
        var addCategoryCommand = serviceProvider.GetCommandHandler<AddCategoryCommandHandler>();

        var args = ParseArgs(callbackQuery);
        var chatId = callbackQuery.Message.Chat.Id;

        var category = await categoryCache.FindAsync(args.Id ?? -1);
        var categories = (await categoryCache.GetAllAsync(cancellationToken)).Where(c => c.ParentId == args.Id);

        var keyboard = keyboardFactory
            .AddButton("➕ Добавить новую", addCategoryCommand.BuildArgs(new AddCategoryArgs() { ParentId = args.Id}));

        if (args.Id != null)
        {
            keyboard.AddButton("✏️ Редактировать", "asd");
        }

        keyboard.AddNewLine();

        if (categories.Any())
        {
            keyboard.AddPaginated(
                items: categories,
                itemFormatter: c => c.Name,
                callbackData: (cat) => BuildArgs(new() { Id = cat.Id}),
                paginateData: (p) => BuildArgs(new() { Id = args.Id, Nav = p}),
                page: args.Nav ?? 1,
                pageSize: 5);
        }

        keyboard.AddBackButton();

        var message = category != null
            ? $"📋 Категория: {category.Name}"
            : "📋 Список категорий транзакций";

        await bot.EditMessageText(chatId,
                    callbackQuery.Message.Id,
                    message,
                    replyMarkup: keyboard.Create(),
                    cancellationToken: cancellationToken);
    }


}
