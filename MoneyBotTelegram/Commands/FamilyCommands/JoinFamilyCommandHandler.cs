using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = MoneyBotTelegram.Infrasctructure.Entities.User;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public enum JoinFamilyState
{
    Idle,
    EnteringUsername
}
public class JoinFamilyCommandHandler(
    IUserService userService,
    IConversationState<JoinFamilyState> conversation,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_join", "Присоединиться к семье");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From.Id) != JoinFamilyState.Idle;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        //var user = message.From!;
        //var text = message.Text!;

        //var dbUser = await userService.GetAsync(user.Id);

        //if (dbUser == null)
        //{
        //    await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
        //    return;
        //}

        //if (dbUser.FamilyParent != null)
        //{
        //    await bot.SendMessage(message.Chat.Id, "Вы уже присоединены к семье");
        //    return;
        //}

        //var state = conversation.GetState(user.Id);

        //switch (state)
        //{
        //    case JoinFamilyState.Idle:
        //        conversation.SetState(user.Id, JoinFamilyState.EnteringUsername);

        //        await bot.SendMessage(message.Chat.Id, "Введите имя пользователя или его Id");
        //        break;
        //    case JoinFamilyState.EnteringUsername:
        //        User? parentUser = null;

        //        if (long.TryParse(text, out var parentUserId))
        //        {
        //            parentUser = await userService.GetAsync(parentUserId);
        //        }
        //        else
        //        {
        //            parentUser = await userService.GetAsync(text);
        //        }

        //        if (parentUser == null || parentUser.FamilyParentId != null)
        //        {
        //            await bot.SendMessage(message.Chat.Id, "Указанный пользователь не найден");
        //            return;
        //        }

        //        dbUser.FamilyParentId = parentUser.Id;

        //        await userService.SaveAsync(dbUser);

        //        conversation.SetState(user.Id, JoinFamilyState.Idle);

        //        await bot.SendMessage(parentUser.Id, $"{dbUser.FirstName} присоединился к вашей семье");
        //        await bot.SendMessage(message.Chat.Id, "Вы успешно присоеденились к семье");

        //        break;
        //}
    }

    //private async Task<BotResponse> JoinToFamily(User user, string idOrUsername)
    //{
    //    User? parentFamilyUser;

    //    if (long.TryParse(idOrUsername, out var userId))
    //    {
    //        parentFamilyUser = await userService.GetAsync(userId);
    //    }
    //    else
    //    {
    //        parentFamilyUser = await userService.GetAsync(idOrUsername);
    //    }

    //    if(parentFamilyUser == null)
    //    {
    //        return new($"Пользователь {idOrUsername} не найден");
    //    }

    //    if(parentFamilyUser.FamilyParent != null)
    //    {
    //        return new($"Невозможно присоединиться к семье {idOrUsername}, так как он не является ее владельцем");
    //    }

    //    user.FamilyParent = parentFamilyUser;

    //    await userService.SaveAsync(user);

    //    stateStore.SetUserState(Command, user.Id, UserState.None);

    //    await userService.SendMessageAsync(parentFamilyUser.Id, $"Пользователь {user.FirstName} присоединился к вашей семье");

    //    return new($"Вы успешно присоеденились к семье {parentFamilyUser.FirstName}");
    //}
}
