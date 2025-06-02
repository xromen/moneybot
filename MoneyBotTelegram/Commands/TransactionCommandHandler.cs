//using Microsoft.AspNetCore.Mvc.RazorPages;
//using MoneyBotTelegram.CallbackQueries.Common;
//using MoneyBotTelegram.Commands.Common;
//using MoneyBotTelegram.Common;
//using MoneyBotTelegram.Infrasctructure;
//using MoneyBotTelegram.Infrasctructure.Entities;
//using MoneyBotTelegram.Services;
//using Telegram.Bot.Types;

//namespace MoneyBotTelegram.Commands;

//public class TransactionCommandHandler(
//    IKeyboardFactory keyboardFactory,
//    IEntityCacheService<Category> categoryCache,
//    IEntityCacheService<Place> placeCache,
//    ApplicationDbContext db,
//    IUserStateStore stateStore
//    ) : IBotCommandHandler, ICommandMetadata
//{
//    public static CommandMetadata Metadata { get; } = new("/transaction", "Добавить новую транзакцию");

//    public string Command => Metadata.Command;


//    private string MakeCommand(decimal? amount = null, long? categoryId = null, DateTime? date = null, long? placeId = null)
//    {
//        return CallbackDataBuilder.Build(Command, new
//        {
//            Amount = amount,
//            CategoryId = categoryId,
//            Date = date,
//            PlaceId = placeId
//        }, ' ');
//    }
//    private string MakePaginateCommand(string property, int page)
//    {
//        return $"{Command} property {property} page {page}";
//    }
//    private static Dictionary<long, TransactionInputState> _userInputsState = new Dictionary<long, TransactionInputState>();

//    public async Task<BotResponse> HandleAsync(Message message, CancellationToken cancellationToken)
//    {
//        long userId = message.From.Id;
//        string text = message.Text;
//        var splitted = text.Split(' ');

//        if (!_userInputsState.ContainsKey(userId) || (splitted.Length == 2 && splitted[1] == "clear"))
//        {
//            _userInputsState[userId] = new();
//        }

//        _userInputsState.TryGetValue(userId, out var inputState);

//        IKeyboardFactory keyboard = keyboardFactory.Empty();
//        string responseMessage = string.Empty;

//        if(splitted.Length == 2 && splitted[1] == "save")
//        {
//            if(inputState.Amount.HasValue && inputState.Category != null)
//            {
//                var transaction = new Transaction()
//                {
//                    Amount = inputState.Amount.Value,
//                    Date = DateOnly.FromDateTime(inputState.Date),
//                    Categoryid = inputState.Category.Id,
//                    PlaceId = inputState.Place?.Id,
//                    UserId = userId
//                };

//                await db.Transactions.AddAsync(transaction);
//                await db.SaveChangesAsync();

//                if(inputState.Amount > 0)
//                {
//                    responseMessage += "✅ Доход успешно добавлен";
//                }
//                else
//                {
//                    responseMessage += "✅ Расход успешно добавлен";
//                }

//                stateStore.SetUserState(Command, userId, UserState.None);
//                return new(responseMessage, SaveInHistory: false);
//            }
//            else
//            {
//                responseMessage += "❌ Ошибка сохранения, не заполнены обязательные поля: Сумма или Категория\n";
//            }
//        } 

//        //var parameters = text.Split(' ');

//        var dataParser = new CallbackDataParser(text, Command, ' ');
//        var categoryId = dataParser.GetLong("CategoryId");
//        var placeId = dataParser.GetLong("PlaceId");

//        var property = dataParser.GetString("property");

//        //Если пришло значение categoryId или placeId
//        if (categoryId.HasValue)
//        {
//            var category = await categoryCache.FindAsync(categoryId.Value);

//            inputState.Category = category;
//            stateStore.SetUserState(Command, userId, UserState.None);
//        }
//        if(placeId.HasValue)
//        {
//            var place = await placeCache.FindAsync(placeId.Value);

//            inputState.Place = place;
//            stateStore.SetUserState(Command, userId, UserState.None);
//        }

//        //Если ждем ввода с клавитуры
//        switch (inputState.Stage)
//        {
//            case TransactionInputStage.Amount:
//                if(decimal.TryParse(text, out var amount))
//                {
//                    inputState.Amount = amount;
//                    inputState.Stage = TransactionInputStage.None;
//                    stateStore.SetUserState(Command, userId, UserState.None);
//                }
//                else
//                {
//                    inputState.Stage = TransactionInputStage.None;
//                    property = nameof(TransactionInputState.Amount);
//                    responseMessage += "❌ Введено неверное значение!\n";
//                }
//                break;
//            case TransactionInputStage.Date:
//                if (DateTime.TryParse(text, out var date))
//                {
//                    inputState.Date = date;
//                    //stateStore.SetUserState(Command, userId, UserState.None);
//                }
//                else
//                {
//                    property = nameof(TransactionInputState.Date);
//                    responseMessage += "❌ Введено неверное значение!\n";
//                }
//                inputState.Stage = TransactionInputStage.None;
//                break;
//        }

//        if (string.IsNullOrEmpty(property) && inputState.Stage == TransactionInputStage.None)
//        {
//            responseMessage += GetStatus(inputState);

//            keyboard.AddButton("💲 Ввести сумму", Command + " property " + nameof(TransactionInputState.Amount)).AddNewLine();
//            keyboard.AddButton("📋 Выбрать категорию", Command + " property " + nameof(TransactionInputState.Category)).AddNewLine();
//            keyboard.AddButton("📅 Выбрать дату", Command + " property " + nameof(TransactionInputState.Date)).AddNewLine();
//            keyboard.AddButton("🏦 Выбрать место", Command + " property " + nameof(TransactionInputState.Place)).AddNewLine();
//            keyboard.AddButton("🗑 Очистить", Command + " clear").AddButton("✅ Добавить", Command + " save").AddNewLine();
//            keyboard.AddBackButton().AddNewLine();
//        }
//        else if(inputState.Stage == TransactionInputStage.None)
//        {
//            switch (property)
//            {
//                case nameof(TransactionInputState.Amount):
//                    {
//                        stateStore.SetUserState(Command, userId, UserState.WaitingArgument);
//                        inputState.Stage = TransactionInputStage.Amount;
//                        responseMessage += "💲 Введите сумму:";
//                        break;
//                    }
//                case nameof(TransactionInputState.Category):
//                    {
//                        //inputState.Stage = TransactionInputStage.Category;

//                        var categories = await categoryCache.GetAllAsync();
//                        var page = dataParser.GetInt("page") ?? 1;

//                        keyboard.AddPaginated(
//                            categories,
//                            (i) => i.Name,
//                            (i) => MakeCommand(categoryId: i.Id),
//                            (p) => MakePaginateCommand(nameof(TransactionInputState.Category), p),
//                            page,
//                            5);

//                        responseMessage += "📋 Выберите категорию:";
//                        break;
//                    }
//                case nameof(TransactionInputState.Date):
//                    {
//                        stateStore.SetUserState(Command, userId, UserState.WaitingArgument);
//                        inputState.Stage = TransactionInputStage.Date;
//                        responseMessage += "📅 Введите дату формата dd.MM.yyyy:";
//                        break;
//                    }
//                case nameof(TransactionInputState.Place):
//                    {
//                        var places = await placeCache.GetAllAsync();
//                        var page = dataParser.GetInt("page") ?? 1;

//                        keyboard.AddPaginated(
//                            places,
//                            (i) => i.Name,
//                            (i) => MakeCommand(placeId: i.Id),
//                            (p) => MakePaginateCommand(nameof(TransactionInputState.Place), p),
//                            page,
//                            5);

//                        responseMessage += "🏦 Выберите место:";
//                        break;
//                    }
//            }
//        }

//        stateStore.SetUserState(Command, userId, UserState.WaitingArgument);
//        return new(responseMessage, keyboard.IsEmpty() ? null : keyboard.Create());
//    }

//    private string GetStatus(TransactionInputState state)
//    {
//        return $"""
//            💲 Сумма: {state.Amount}
//            📋 Категория: {(state.Category == null ? "не выбрана" : state.Category.Name)}
//            📅 Дата: {state.Date:dd.MM.yyyy}
//            🏪 Место: {(state.Place == null ? "не выбрано" : state.Place.Name)}
//            """;
//    }
//    private enum TransactionInputStage
//    {
//        None,
//        InProgress,
//        Amount,
//        Category,
//        Date,
//        Place,
//        Completed
//    }

//    private class TransactionInputState
//    {
//        public TransactionInputStage Stage { get; set; } = TransactionInputStage.None;
//        public decimal? Amount { get; set; }
//        public Category? Category { get; set; }
//        public DateTime Date { get; set; } = DateTime.Now;
//        public Place? Place { get; set; }
//    }
//}
