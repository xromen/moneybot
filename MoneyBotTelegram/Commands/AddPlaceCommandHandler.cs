//using MoneyBotTelegram.CallbackQueries;
//using MoneyBotTelegram.Commands.Common;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Infrasctructure;
//using MoneyBotTelegram.Infrasctructure.Entities;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;

//namespace MoneyBotTelegram.Commands;

//public class AddPlaceCommandHandler(
//    IUserService userService,
//    IUserStateStore stateStore,
//    IEntityCacheService<Place> placeCache,
//    ILogger<JoinCommandHandler> logger) : IBotCommandHandler, ICommandMetadata
//{
//    public static CommandMetadata Metadata => new("/place_add", "Добавить место");

//    public string Command => Metadata.Command;

//    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
//    {
//        var user = message.From!;
//        var text = message.Text!;

//        var dbUser = await userService.GetAsync(user.Id);

//        if (dbUser == null)
//        {
//            return new("Вначале необходимо зарегистрироваться!");
//        }

//        var state = stateStore.GetUserState(user.Id);

//        if(state != null && state.UserState == UserState.WaitingArgument)
//        {
//            await placeCache.AddAsync(new Place()
//            {
//                Name = text,
//            });

//            stateStore.SetUserState(Command, user.Id, UserState.None);

//            return new("Место добавлено", SaveInHistory: false, RedirectCallbackData: PlaceCallbackHandler.MakeCallbackData());
//        }
//        else
//        {
//            stateStore.SetUserState(Command, user.Id, UserState.WaitingArgument);

//            return new("Введите название места", SaveInHistory: false);
//        }
//    }
//}
