//using Microsoft.EntityFrameworkCore;
//using MoneyBotTelegram.CallbackQueries.Common;
//using MoneyBotTelegram.Commands;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Infrasctructure;
//using MoneyBotTelegram.Infrasctructure.Entities;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace MoneyBotTelegram.CallbackQueries;

//public class CategoryCallbackHandler(
//    IUserService userService, 
//    IEntityCacheService<Category> categoryCache, 
//    IKeyboardFactory keyboardFactory) : ICallbackQueryHandler
//{
//    public const string CategoryPrefix = "cbcategory";
//    public const char separator = ' ';

//    /// <summary>
//    /// При id=-1 Вывод категорий без родителя
//    /// </summary>
//    /// <param name="id"></param>
//    /// <returns></returns>
//    public static string MakeCallbackData(long id = -1, int page = 1)
//    {
//        return CallbackDataBuilder.Build(CategoryPrefix, new
//        {
//            id = id,
//            nav = page
//        }, separator);
//    }

//    private string MakeCallbackData(Category category)
//    {
//        return MakeCallbackData(category.Id);
//    }

//    public bool CanHandle(string callbackData)
//    {
//        return callbackData.StartsWith(CategoryPrefix, StringComparison.OrdinalIgnoreCase);
//    }

//    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
//    {
//        var userId = callbackQuery.From.Id;
//        var user = await userService.GetAsync(userId, cancellationToken);

//        var dataParser = new CallbackDataParser(callbackQuery.Data, CategoryPrefix, separator);

//        var categoryId = dataParser.GetLong("id") ?? -1;
//        var page = dataParser.GetInt("nav") ?? 1;

//        var parentCategory = await categoryCache.FindAsync(categoryId);

//        var categories = await GetChildCategoriesAsync(categoryId, cancellationToken);

//        var keyboard = BuildKeyboard(categories, parentCategory, page, categoryId);

//        var message = parentCategory != null
//            ? $"📋 Категория: {parentCategory.Name}"
//            : "📋 Список категорий транзакций";

//        return new BotResponse(message, keyboard);
//    }

//    private async Task<IEnumerable<Category>> GetChildCategoriesAsync(long parentId, CancellationToken cancellationToken)
//    {
//        var categories = await categoryCache.GetAllAsync();
//        if (parentId == -1)
//        {
//            return categories
//                .Where(c => c.ParentId == null);
//        }

//        return categories
//            .Where(c => c.ParentId == parentId);
//    }

//    private InlineKeyboardMarkup BuildKeyboard(
//        IEnumerable<Category> categories,
//        Category? parentCategory,
//        int page,
//        long categoryId)
//    {
//        var keyboard = keyboardFactory
//            .AddButton("➕ Добавить новую", AddCategoryCommandHandler.MakeCommand(categoryId));

//        if (parentCategory != null)
//        {
//            keyboard.AddButton("✏️ Редактировать", "asd");
//        }

//        keyboard.AddNewLine();

//        if (categories.Any())
//        {
//            keyboard.AddPaginated(
//                items: categories,
//                itemFormatter: c => c.Name,
//                callbackData: MakeCallbackData,
//                paginateData: (p) => MakeCallbackData(categoryId, p),
//                page: page,
//                pageSize: 5);
//        }

//        keyboard.AddBackButton();

//        return keyboard.Create();
//    }
//}
