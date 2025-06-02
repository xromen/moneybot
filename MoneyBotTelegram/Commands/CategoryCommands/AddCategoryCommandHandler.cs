using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands.CategoryCommands;

public class AddCategoryArgs
{
    public string? Name { get; set; }
    public long? ParentId { get; set; }
}

public enum State
{
    Idle = 0,
    EnteringName = 1
}

public class AddCategoryCommandHandler(
    IUserService userService,
    IDraftService<Category> draft,
    IConversationState<State> conversation,
    IEntityCacheService<Category> categoryCache,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<AddCategoryArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/category_add", "Добавить категорию");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From!.Id) != State.Idle;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken)
    {
        var user = message.From!;
        var text = message.Text!;

        var dbUser = await userService.GetAsync(user.Id);

        if (dbUser == null)
        {
            await bot.SendMessage(message.Chat.Id, GlobalConstants.NeedRegisterMessage);
            return;
        }

        var state = conversation.GetState(user.Id);

        switch (state)
        {
            case State.Idle:
                var args = ParseArgs(message);

                conversation.SetState(user.Id, State.EnteringName);
                draft.UpdateDraft(user.Id, c => c.ParentId = args.ParentId);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите наименование категории");
                return;

            case State.EnteringName:
                var category = draft.GetDraft(user.Id);
                category.Name = text;

                await categoryCache.AddAsync(category);

                conversation.SetState(user.Id, State.Idle);

                draft.ClearDraft(user.Id);

                var keyboard = await keyboardFactory.CreateDefault(user.Id);

                await bot.SendMessage(
                    message.Chat.Id,
                    "✅ Категория успешно добавлена",
                    replyMarkup: keyboard);
                return;
        }

        //var parameters = text.Split(' ');

        //if (parameters.Length == 2 && parameters[0].Equals(Command, StringComparison.OrdinalIgnoreCase))
        //{
        //    if (long.TryParse(parameters[1], out var parentId))
        //    {
        //        _userParentCategory[user.Id] = parentId;
        //    }
        //    else
        //    {
        //        return new("Неверно передан Id родительской категории");
        //    }

        //    stateStore.SetUserState(Command, user.Id, UserState.WaitingArgument);

        //    return new("Введите название новой категории", SaveInHistory: false);
        //}

        //if (stateStore.GetUserState(user.Id)?.UserState != UserState.None)
        //{
        //    var parentId = _userParentCategory[user.Id];
        //    //Category? parentCategory = parentId == -1 ? null : await categoryCache.FindAsync(parentId);

        //    var category = new Category() { Name = text, ParentId = parentId == -1 ? null : parentId };

        //    await categoryCache.AddAsync(category);

        //    stateStore.SetUserState(Command, user.Id, UserState.None);

        //    return new("Категория создана", RedirectCallbackData: CategoryCallbackHandler.MakeCallbackData(parentId), SaveInHistory: false);
        //}

        //return new("Ошибка");
    }
}
