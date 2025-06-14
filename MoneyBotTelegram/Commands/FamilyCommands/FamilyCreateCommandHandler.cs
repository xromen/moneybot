using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public class FamilyCreateArgs : BaseArgs
{

}

public class FamilyCreateCommandHandler(
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    IServiceProvider serviceProvider,
    ILogger<JoinCommandHandler> logger) : BaseCommand<FamilyCreateArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_create", "Создать семью");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var user = message.From!;

        var dbUser = await db.Users.Include(c => c.Family).ThenInclude(c => c.Owner).SingleOrDefaultAsync(c => c.Id == user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        var args = ParseArgs(message);

        var family = new Family() { OwnerId = user.Id };

        await db.Families.AddAsync(family);
        await db.SaveChangesAsync();

        dbUser.FamilyId = family.Id;
        await db.SaveChangesAsync();

        await bot.SendMessage(message.Chat.Id, "✅ Семья успешно создана");
    }
}
