using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public class LeaveFamilyCommandHandler(
    IUserService userService,
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<BaseArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_leave", "Выйти из семьи");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var user = message.From!;

        var dbUser = await db.Users.Include(c => c.Family).SingleOrDefaultAsync(c => c.Id == user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        if (dbUser.Family == null)
        {
            await bot.SendMessage(message.Chat.Id, "У вас нет семьи :(");
            return;
        }

        if(dbUser.Family.OwnerId == dbUser.Id)
        {
            await bot.SendMessage(message.Chat.Id, "Вы не можете покинуть свою семью, вместо этого удалите ее");
            return;
        }

        dbUser.Family = null;
        dbUser.FamilyId = null;

        await userService.SaveAsync(dbUser);

        await bot.SendMessage(message.Chat.Id, "Вы успешно покинули семью");
    }
}

