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

public class FamilyDeleteCommandHandler(
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    IServiceProvider serviceProvider,
    ILogger<JoinCommandHandler> logger) : BaseCommand<BaseArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_delete", "Удалить семью");

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

        if(dbUser.Family == null)
        {
            await bot.SendMessage(message.Chat.Id, "У вас нет семьи :(");
            return;
        }
        if (dbUser.Family.OwnerId != user.Id)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Вы не являетесь владельцем семьи");
            return;
        }

        var family = await db.Families.SingleOrDefaultAsync(c => c.Id == dbUser.FamilyId);

        foreach(var member in family.Members)
        {
            member.Family = null;
            member.FamilyId = null;
        }

        db.Families.Remove(family);

        await db.SaveChangesAsync();

        await bot.SendMessage(message.Chat.Id, "✅ Семья успешно удалена");
    }
}
