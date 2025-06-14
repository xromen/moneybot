using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Commands.FamilyCommands;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.Account;

public class MeCommandHandler(
    IUserService userService,
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory
    ) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata { get; } = new("/me", "Показать информацию о себе");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var userId = message.From?.Id;

        var user = await db.Users.Include(c => c.Family).ThenInclude(c => c.Owner).SingleOrDefaultAsync(c => c.Id == userId);

        if (user == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        var familyOwner = string.Empty;
        var isOwnerFamily = user.Family?.OwnerId == userId;

        if (user.Family != null && user.Family.OwnerId == userId)
        {
            familyOwner = "Вы";
        }
        else if (user.Family != null)
        {
            familyOwner = user.Family.Owner.FirstName;
        }
        else
        {
            familyOwner = "Нет";
        }

        var keyboard = keyboardFactory.Empty();

        keyboard.AddButton("👨‍👩‍👦 Управление семьей", FamilySettingsCommandHandler.Metadata.Command);

        keyboard.AddNewLine()
            .AddBackButton();

        var text = $"""
                    📋 Ваш профиль:
                    ID: {user.Id}
                    Username: @{user.Username}
                    Имя: {user.FirstName}
                    Фамилия: {user.LastName}
                    Дата регистрации: {user.CreatedAt:dd.MM.yyyy}
                    Владелец семьи: {familyOwner}
                    """;

        if (editMessage)
        {
            await bot.EditMessageText(message.Chat.Id, message.Id, text, replyMarkup: keyboard.Create());
        }
        else
        {
            await bot.SendMessage(
                message.Chat.Id,
                text,
                replyMarkup: keyboard.Create()
                );
        }
    }
}
