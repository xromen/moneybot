using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Family;

public class LeaveFamilyCommandHandler(
    IUserService userService,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_leave", "Выйти из семьи");

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

        if (dbUser.FamilyParent == null)
        {
            await bot.SendMessage(message.Chat.Id, "У вас нет семьи :(");
            return;
        }

        dbUser.FamilyParent = null;

        await userService.SaveAsync(dbUser);

        var keyboard = keyboardFactory.AddToMainMenuButton();

        await bot.SendMessage(message.Chat.Id, "Вы успешно покинули семью", replyMarkup: keyboard.Create());
    }
}

