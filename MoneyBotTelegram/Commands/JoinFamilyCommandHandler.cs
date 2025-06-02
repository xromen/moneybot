using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = MoneyBotTelegram.Infrasctructure.Entities.User;

namespace MoneyBotTelegram.Commands;

public class JoinFamilyCommandHandler(
    IUserService userService,
    ILogger<JoinCommandHandler> logger) : BaseCommand, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/family_join", "Присоединиться к семье");

    public override string Command => Metadata.Command;

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        //var user = message.From!;
        //var text = message.Text!;

        //var dbUser = await userService.GetAsync(user.Id);

        //if (dbUser == null)
        //{
        //    return new("Вначале необходимо зарегистрироваться!");
        //}

        //if(dbUser.FamilyParent != null)
        //{
        //    return new("Вы уже присоединены к семье");
        //}

        //var parameters = text.Split(' ');

        //if(parameters.Length == 2)
        //{
        //    return await JoinToFamily(dbUser, parameters[1]);
        //}

        //var state = stateStore.GetUserState(user.Id);
        //if(state != null && state.Command == Command && state.UserState == UserState.WaitingArgument)
        //{
        //    return await JoinToFamily(dbUser, parameters[0]);
        //}
        //else
        //{
        //    stateStore.SetUserState(Command, user.Id, UserState.WaitingArgument);
        //    return new("Введите имя пользователя или id к которому хотите присоединиться");
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
