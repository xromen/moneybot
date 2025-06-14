using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public class FamilyBanishArgs : BaseArgs
{
    public long? UserId { get; set; }
}
public class FamilyBanishCommandHandler(
    IUserService userService,
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<FamilyBanishArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_banish", "Исключить из семьи");

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

        if (dbUser.Family == null || dbUser.Family.OwnerId != dbUser.Id)
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

        var banishUser = await db.Users.Include(c => c.Family).SingleOrDefaultAsync(c => c.Id == args.UserId);

        if (banishUser == null || banishUser.Family == null || banishUser.FamilyId != dbUser.FamilyId)
        {
            await bot.SendMessage(message.Chat.Id, "Ошибка в идентификации члена семьи");
            return;
        }

        banishUser.FamilyId = null;
        banishUser.Family = null;

        await userService.SaveAsync(banishUser);

        await bot.SendMessage(banishUser.Id, $"Пользователь {dbUser.FirstName} исключил вас из своей семьи :(");



        await bot.SendMessage(
            message.Chat.Id,
            $"{banishUser.FirstName} исключен из вашей семьи",
            replyMarkup: keyboardFactory.AddToMainMenuButton().Create());
    }
}
