using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = MoneyBotTelegram.Infrasctructure.Entities.User;

namespace MoneyBotTelegram.Commands;

public class JoinCommandHandler(
    IUserService userService,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/join", "Присоединиться к боту");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        var user = message.From!;

        if(await userService.ExistsAsync(user.Id, cancellationToken))
        {
            await bot.SendMessage(message.Chat.Id, "Вы уже зарегистрированы!");
            return;
        }

        var newUser = new User
        {
            Id = user.Id,
            Username = user.Username ?? "Аноним",
            FirstName = user.FirstName,
            LastName = user.LastName,
            CreatedAt = DateTime.UtcNow,
        };

        await userService.SaveAsync(newUser, cancellationToken);
        logger.LogInformation("New user joined: {UserId}", user.Id);

        await bot.SendMessage(message.Chat.Id, $"Добро пожаловать, {newUser.FirstName}! 🎉");
    }
}
