//using MoneyBotTelegram.Commands.Common;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Services;
//using Telegram.Bot;
//using Telegram.Bot.Types;

//namespace MoneyBotTelegram.Commands;

//public class FamilyBanishCommandHandler(
//    IUserService userService,
//    ILogger<JoinCommandHandler> logger) : IBotCommandHandler, ICommandMetadata
//{
//    public static CommandMetadata Metadata => new("/family_banish", "Исключить из семьи");

//    public string Command => Metadata.Command;

//    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
//    {
//        var user = message.From!;

//        var dbUser = await userService.GetAsync(user.Id);

//        if(dbUser == null)
//        {
//            return new("Вначале необходимо зарегистрироваться");
//        }

//        var family = await userService.GetYourFamilyAsync(user.Id);

//        if (!family.Any())
//        {
//            return new("У вас нет семьи или вы не являетесь ее владельцем");
//        }

//        var parameters = message.Text!.Split(' ');

//        if(parameters.Length != 2)
//        {
//            return new($"Неверное использование команды. Используйте /{HelpCommandHandler.Metadata.Command}");
//        }

//        long banishUserId;

//        if (!long.TryParse(parameters[1], out banishUserId))
//        {
//            return new("Ошибка в идентификации члена семьи");
//        }

//        var banishUser = await userService.GetAsync(banishUserId);

//        if (banishUser == null || (banishUser.FamilyParent != null && banishUser.FamilyParent.Id != user.Id))
//        {
//            return new("Ошибка в идентификации члена семьи");
//        }

//        banishUser.FamilyParent = null;

//        await userService.SaveAsync(banishUser);

//        await userService.SendMessageAsync(banishUser.Id, $"Пользователь {dbUser.FirstName} исключил вас из своей семьи :(");

//        return new($"{banishUser.FirstName} исключен из вашей семьи");
//    }
//}
