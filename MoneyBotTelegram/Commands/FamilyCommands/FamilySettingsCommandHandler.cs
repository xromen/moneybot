using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public class FamilySettingsCommandHandler(
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    IServiceProvider serviceProvider,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_settings", "Управление семьей");

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

        var keyboard = keyboardFactory.Empty();
        StringBuilder responseTextBuilder = new StringBuilder("👨‍👩‍👦 Управление семьей\n");

        if (dbUser.Family == null)
        {
            var familyCreateCommandHandler = serviceProvider.GetCommandHandler<FamilyCreateCommandHandler>();

            responseTextBuilder.AppendLine("У вас нет семьи :(");

            keyboard.AddButton("➕ Создать новую семью", familyCreateCommandHandler.BuildArgs(new() { Redirect = Command})).AddNewLine();
        }
        else if (dbUser.Family.OwnerId == dbUser.Id)
        {
            responseTextBuilder.AppendLine($"Глава семьи: {dbUser.FirstName} (Вы)");
            var members = await db.Users.Include(c => c.Family).Where(c => c.FamilyId == dbUser.FamilyId && c.Id != dbUser.Id).ToListAsync();

            var familyBanishCommandHandler = serviceProvider.GetCommandHandler<FamilyBanishCommandHandler>();

            foreach (var person in members)
            {
                keyboard.AddButton(person.Username + " " + person.FirstName + " " + person.LastName, familyBanishCommandHandler.BuildArgs(new() { UserId = person.Id }));
                keyboard.AddNewLine();
            }

            var addFamilyMemberData = ArgsParser<BaseArgs>.BuildArgs(new BaseArgs() { Redirect = Command }, AddFamilyMemberCommandHandler.Metadata.Command);
            keyboard.AddButton("➕ Добавить", addFamilyMemberData).AddNewLine();

            var familyDeleteData = ArgsParser<BaseArgs>.BuildArgs(new BaseArgs() { Redirect = Command }, FamilyDeleteCommandHandler.Metadata.Command);
            keyboard.AddButton("❌ Удалить семью", familyDeleteData).AddNewLine();

            responseTextBuilder.AppendLine("\n🗑 Нажмите на участника для исключения");
        }
        else
        {
            var members = await db.Users.Include(c => c.Family).Where(c => c.FamilyId == dbUser.FamilyId && c.Id != dbUser.Family.OwnerId).ToListAsync();

            responseTextBuilder.AppendLine($"Глава семьи: {dbUser.Family.Owner.FirstName}");
            responseTextBuilder.AppendLine("Члены семьи:");
            
            foreach(var person in members)
            {
                responseTextBuilder.AppendLine($"    {person.FirstName}");
            }

            var leaveFamilyData = ArgsParser<BaseArgs>.BuildArgs(new BaseArgs() { Redirect = Command }, LeaveFamilyCommandHandler.Metadata.Command);
            keyboard.AddButton("❌ Покинуть семью", leaveFamilyData).AddNewLine();
        }


        keyboard.AddBackButton();

        await bot.SendMessage(message.Chat.Id, responseTextBuilder.ToString(), replyMarkup: keyboard.Create());
    }
}
