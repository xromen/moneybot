//using MoneyBotTelegram.Commands;

//namespace MoneyBotTelegram.Services;

//public interface IUserNavigationService
//{
//    Task SetCurrent(long userId, string callbackData);
//    Task<string?> PopAsync(long userId);
//    Task ClearAsync(long userId);
//}

//public class UserNavigationService : IUserNavigationService
//{
//    private static Dictionary<long, Stack<string>> _userHistories = new();
//    private static Dictionary<long, string> _currDatas = new();

//    public Task ClearAsync(long userId)
//    {
//        _userHistories.Remove(userId);
//        return Task.CompletedTask;
//    }

//    public Task<string> PopAsync(long userId)
//    {
//        if (_userHistories.TryGetValue(userId, out var stack) && stack.Count > 0)
//        {
//            var data = stack.Pop();
//            _currDatas[userId] = data;
//            return Task.FromResult(data);
//        }
//        else
//        {
//            _currDatas[userId] = StartCommandHandler.Metadata.Command;
//            return Task.FromResult(StartCommandHandler.Metadata.Command);
//        }
//    }

//    public Task SetCurrent(long userId, string callbackData)
//    {
//        if (!_userHistories.ContainsKey(userId))
//            _userHistories[userId] = new Stack<string>();

//        _currDatas.TryGetValue(userId, out var currentData);

//        if (currentData != null && currentData.Split(' ')[0] != callbackData.Split(' ')[0])
//        {
//            _userHistories[userId].Push(currentData);
//        }

//        _currDatas[userId] = callbackData;

//        return Task.CompletedTask;
//    }
//}

//public class UserResponseHistory(IKeyboardFactory keyboardFactory)
//{

//    private static Dictionary<long, Stack<BotResponse>> _responseHistory = new();
//    private static Dictionary<long, BotResponse> _currResponse = new();
//    public void AddNewResponse(long userId, BotResponse newResponse)
//    {
//        if (!_responseHistory.ContainsKey(userId))
//            _responseHistory[userId] = new Stack<BotResponse>();

//        if (_currResponse.TryGetValue(userId, out var response))
//        {
//            _responseHistory[userId].Push(response);
//        }

//        _currResponse[userId] = newResponse;
//    }

//    public async Task<BotResponse> GetLastResponse(long userId)
//    {
//        if (_responseHistory.TryGetValue(userId, out var stack) && stack.Count > 0)
//        {
//            var response = stack.Pop();
//            _currResponse[userId] = response;
//            return response;
//        }
//        else
//        {
//            return new BotResponse("Главное меню:", await keyboardFactory.CreateDefault());
//        }
//    }
//}
