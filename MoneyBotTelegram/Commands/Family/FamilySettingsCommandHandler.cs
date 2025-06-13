using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Family;

public class FamilySettingsCommandHandler(
    IUserService userService,
    IKeyboardFactory keyboardFactory,
    IServiceProvider serviceProvider,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_settings", "Управление семьей");

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

        if (dbUser.FamilyParent != null)
        {
            await bot.SendMessage(message.Chat.Id, "Вы не владелец семьи");
            return;
        }

        var family = await userService.GetYourFamilyAsync(user.Id);
        //if (!family.Any())
        //{
        //    await bot.SendMessage(message.Chat.Id, "У вас нет семьи :(");
        //    return;
        //}

        var keyboard = keyboardFactory.Empty();
        var familyBanishCommandHandler = serviceProvider.GetCommandHandler<FamilyBanishCommandHandler>();

        foreach (var person in family)
        {
            keyboard.AddButton(person.Username + " " + person.FirstName + " " + person.LastName, familyBanishCommandHandler.BuildArgs(new() { UserId = person.Id }));
            keyboard.AddNewLine();
        }

        keyboard.AddButton("➕ Добавить", AddFamilyMemberCommandHandler.Metadata.Command).AddNewLine();

        keyboard.AddBackButton();

        await bot.SendMessage(message.Chat.Id, "Управление семьей. \nНажмите на участника для исключения", replyMarkup: keyboard.Create());
    }
}
