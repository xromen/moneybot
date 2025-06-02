//using MoneyBotTelegram.Commands.Common;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;

//namespace MoneyBotTelegram.Commands;

//public class LeaveFamilyCommandHandler(
//    IUserService userService,
//    ILogger<JoinCommandHandler> logger) : IBotCommandHandler, ICommandMetadata
//{
//    public static CommandMetadata Metadata => new("/family_leave", "Выйти из семьи");

//    public string Command => Metadata.Command;

//    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
//    {
//        var user = message.From!;

//        var dbUser = await userService.GetAsync(user.Id);

//        if (dbUser == null)
//        {
//            return new("Вначале необходимо зарегистрироваться!");
//        }

//        if (dbUser.FamilyParent == null)
//        {
//            return new("У вас нет семьи :(");
//        }

//        dbUser.FamilyParent = null;

//        await userService.SaveAsync(dbUser);

//        return new("Вы успешно вышли из семьи");
//    }
//}

