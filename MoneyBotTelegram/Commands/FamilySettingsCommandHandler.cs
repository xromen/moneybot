//using MoneyBotTelegram.Commands.Common;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;

//namespace MoneyBotTelegram.Commands;

//public class FamilySettingsCommandHandler(
//    IUserService userService,
//    IKeyboardFactory keyboardFactory,
//    ILogger<JoinCommandHandler> logger) : IBotCommandHandler, ICommandMetadata
//{
//    public static CommandMetadata Metadata => new("/family_settings", "Управление семьей");

//    public string Command => Metadata.Command;

//    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
//    {
//        var user = message.From!;

//        var dbUser = await userService.GetAsync(user.Id);

//        if (dbUser == null)
//        {
//            return new("Вначале необходимо зарегистрироваться!");
//        }

//        if (dbUser.FamilyParent != null)
//        {
//            return new("Вы не владелец семьи");
//        }

//        var family = await userService.GetYourFamilyAsync(user.Id);
//        if (!family.Any())
//        {
//            return new("У вас нет семьи :(");
//        }

//        var keyboard = keyboardFactory.Empty();

//        foreach(var person in family)
//        {
//            keyboard.AddButton(person.Username + " " + person.FirstName + " " + person.LastName, FamilyBanishCommandHandler.Metadata.Command + " " + person.Id);
//            keyboard.AddNewLine();
//        }

//        keyboard.AddBackButton();

//        return new("Управление семьей. \nНажмите на участника для исключения", keyboard.Create());
//    }
//}
