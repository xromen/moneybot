using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Family;

public class FamilyBanishArgs
{
    public long? UserId { get; set; }
}
public class FamilyBanishCommandHandler(
    IUserService userService,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<FamilyBanishArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_banish", "Исключить из семьи");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var user = message.From!;

        var dbUser = await userService.GetAsync(user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        var family = await userService.GetYourFamilyAsync(user.Id);

        if (!family.Any())
        {
            await bot.SendMessage(message.Chat.Id, "У вас нет семьи или вы не являетесь ее владельцем");
            return;
        }

        var args = ParseArgs(message);

        if (args.UserId == null)
        {
            await bot.SendMessage(message.Chat.Id, $"Неверное использование команды. Используйте /{HelpCommandHandler.Metadata.Command}");
            return;
        }

        var banishUser = await userService.GetAsync(args.UserId.Value);

        if (banishUser == null || banishUser.FamilyParent != null && banishUser.FamilyParent.Id != user.Id)
        {
            await bot.SendMessage(message.Chat.Id, "Ошибка в идентификации члена семьи");
            return;
        }

        banishUser.FamilyParent = null;

        await userService.SaveAsync(banishUser);

        await bot.SendMessage(banishUser.Id, $"Пользователь {dbUser.FirstName} исключил вас из своей семьи :(");



        await bot.SendMessage(
            message.Chat.Id,
            $"{banishUser.FirstName} исключен из вашей семьи",
            replyMarkup: keyboardFactory.AddToMainMenuButton().Create());
    }
}
