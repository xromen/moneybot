using MoneyBotTelegram.Commands.Account;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MoneyBotTelegram.Commands;

public class AddCategoryArgs : BaseArgs
{
    public string? Name { get; set; }
    public long? ParentId { get; set; }
}

public enum AddCategoryState
{
    Idle = 0,
    EnteringName = 1
}

public class AddCategoryCommandHandler(
    IUserService userService,
    IDraftService<Category> draft,
    IConversationState<AddCategoryState> conversation,
    IEntityCacheService<Category> categoryCache,
    IKeyboardFactory keyboardFactory,
    ILogger<JoinCommandHandler> logger) : BaseCommand<AddCategoryArgs>, ICommandMetadata
{
    public static CommandMetadata Metadata => new("/category_add", "Добавить категорию");

    public override string Command => Metadata.Command;

    public override bool CanHandle(Message message)
    {
        return base.CanHandle(message) || conversation.GetState(message.From!.Id) != AddCategoryState.Idle;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
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
            case AddCategoryState.Idle:
                var args = ParseArgs(message);

                conversation.SetState(user.Id, AddCategoryState.EnteringName);
                draft.UpdateDraft(user.Id, c => c.ParentId = args.ParentId);

                await bot.SendMessage(
                    message.Chat.Id,
                    "Введите наименование категории");
                return;

            case AddCategoryState.EnteringName:
                var category = draft.GetDraft(user.Id);
                category.Name = text;

                await categoryCache.AddAsync(category);

                conversation.SetState(user.Id, AddCategoryState.Idle);

                draft.ClearDraft(user.Id);

                var keyboard = await keyboardFactory.CreateDefault(user.Id);

                await bot.SendMessage(
                    message.Chat.Id,
                    "✅ Категория успешно добавлена",
                    replyMarkup: keyboard);
                return;
        }
    }
}
