//using MoneyBotTelegram.CallbackQueries.Common;
//using MoneyBotTelegram.Commands;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Infrasctructure.Entities;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;
//using Telegram.Bot.Types.ReplyMarkups;

//namespace MoneyBotTelegram.CallbackQueries;

//public class PlaceCallbackHandler(
//    IUserService userService,
//    IEntityCacheService<Place> placeCache,
//    IKeyboardFactory keyboardFactory) : ICallbackQueryHandler
//{
//    public const string PlacePrefix = "cbplace";
//    public const char separator = ' ';

//    /// <summary>
//    /// При id=-1 Вывод категорий без родителя
//    /// </summary>
//    /// <param name="id"></param>
//    /// <returns></returns>
//    public static string MakeCallbackData(long? id = null, int page = 1)
//    {
//        return CallbackDataBuilder.Build(PlacePrefix, new
//        {
//            id = id,
//            nav = page
//        });
//    }

//    public bool CanHandle(string callbackData)
//    {
//        return callbackData.StartsWith(PlacePrefix, StringComparison.OrdinalIgnoreCase);
//    }

//    public async Task<BotResponse> HandleCallbackAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
//    {
//        var userId = callbackQuery.From.Id;
//        var user = await userService.GetAsync(userId, cancellationToken);

//        var dataParser = new CallbackDataParser(callbackQuery.Data, PlacePrefix, separator);

//        var placeId = dataParser.GetLong("id");
//        var page = dataParser.GetInt("nav") ?? 1;

//        var places = await placeCache.GetAllAsync(cancellationToken);

//        var keyboard = BuildKeyboard(places, page, placeId);

//        var message = placeId != null
//            ? $"🏪 Место: {places.Single(c => c.Id == placeId).Name}"
//            : "🏪 Список мест:";

//        return new BotResponse(message, keyboard);
//    }

//    private InlineKeyboardMarkup BuildKeyboard(
//        IEnumerable<Place> places,
//        int page,
//        long? placeId = null)
//    {
//        if(placeId != null)
//        {
//            var keyboard = keyboardFactory
//                .AddButton("✏️ Редактировать", "asd").AddNewLine()
//                .AddBackButton();

//            return keyboard.Create();
//        }
//        else
//        {
//            var keyboard = keyboardFactory
//                .AddButton("➕ Добавить новое", AddPlaceCommandHandler.Metadata.Command).AddNewLine();

//            keyboard.AddPaginated(
//                items: places,
//                itemFormatter: c => c.Name,
//                callbackData: (p) => MakeCallbackData(p.Id),
//                paginateData: (p) => MakeCallbackData(page: p),
//                page: page,
//                pageSize: 5);

//            keyboard.AddBackButton();

//            return keyboard.Create();
//        }

//    }
//}
