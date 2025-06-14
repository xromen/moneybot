using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = MoneyBotTelegram.Infrasctructure.Entities.User;

namespace MoneyBotTelegram.Commands.FamilyCommands;

public enum AddFamilyMemberState
{
    Idle,
    EnteringUsername
}

public class AddFamilyMemberArgs : BaseArgs
{
    public bool? Cancel { get; set; }
}

public class AddFamilyMemberCommandHandler(
    IUserService userService,
    ApplicationDbContext db,
    IKeyboardFactory keyboardFactory,
    IConversationState<AddFamilyMemberState> conversation,
    ILogger<JoinCommandHandler> logger) : BaseCommand<AddFamilyMemberArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/add_family_member", "Добавить участника семьи");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From.Id) != AddFamilyMemberState.Idle;
    }

    public override bool CanRedirect(Message message)
    {
        return conversation.GetState(message.From.Id) == AddFamilyMemberState.Idle;
    }


    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var user = message.From!;
        var text = message.Text!;

        var dbUser = await db.Users.Include(c => c.Family).SingleOrDefaultAsync(c => c.Id == user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        if (dbUser.Family != null && dbUser.Family.OwnerId != dbUser.Id)
        {
            await bot.SendMessage(message.Chat.Id, "Вы не глава семьи");
            return;
        }

        var args = ParseArgs(message);

        if (args != null && args.Cancel.HasValue && args.Cancel.Value)
        {
            conversation.SetState(user.Id, AddFamilyMemberState.Idle);

            var redirectData = PopRedirect(user.Id);

            if (!string.IsNullOrEmpty(redirectData))
            {
                message.Text = BuildArgs(new() { Redirect = redirectData });
            }

            return;
        }

        var state = conversation.GetState(user.Id);

        var cancelKeyboard = keyboardFactory.AddButton("🚫 Отменить", BuildArgs(new() { Cancel = true })).Create();

        switch (state)
        {
            case AddFamilyMemberState.Idle:
                conversation.SetState(user.Id, AddFamilyMemberState.EnteringUsername);

                if(args != null && !string.IsNullOrEmpty(args.Redirect))
                {
                    SaveRedicrect(user.Id, args.Redirect);
                }

                await bot.SendMessage(message.Chat.Id, "Введите имя пользователя или его Id", replyMarkup: cancelKeyboard);
                break;
            case AddFamilyMemberState.EnteringUsername:
                User? member = null;

                if (long.TryParse(text, out var memberUserId))
                {
                    member = await db.Users.Include(c => c.Family).SingleOrDefaultAsync(c => c.Id == memberUserId);
                }
                else
                {
                    member = await db.Users.Include(c => c.Family).SingleOrDefaultAsync(c => c.Username == text);
                }

                if (member == null || member.Family != null)
                {
                    await bot.SendMessage(message.Chat.Id, "Указанный пользователь не найден", replyMarkup: cancelKeyboard);
                    return;
                }

                member.FamilyId = dbUser.FamilyId;

                await userService.SaveAsync(member);

                conversation.SetState(user.Id, AddFamilyMemberState.Idle);

                var redirectData = PopRedirect(user.Id);
                if (!string.IsNullOrEmpty(redirectData))
                {
                    message.Text = BuildArgs(new() { Redirect = redirectData });
                }

                await bot.SendMessage(member.Id, $"{dbUser.FirstName} добавил вас в свою семью");
                await bot.SendMessage(message.Chat.Id, $"Вы успешно добавили {member.FirstName} в свою семью");

                break;
        }
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
