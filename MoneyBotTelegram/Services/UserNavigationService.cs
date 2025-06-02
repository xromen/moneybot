using Telegram.Bot.Types;

namespace MoneyBotTelegram.Services;

public interface IUserNavigationService
{
    Task SetCurrent(long userId, Update update);
    Task<Update?> PopAsync(long userId);
    Task ClearAsync(long userId);
}
public class UserNavigationService : IUserNavigationService
{
    private static Dictionary<long, Stack<Update>> _userHistories = new();
    private static Dictionary<long, Update> _currDatas = new();

    public Task ClearAsync(long userId)
    {
        _userHistories.Remove(userId);
        return Task.CompletedTask;
    }

    public Task<Update?> PopAsync(long userId)
    {
        if (_userHistories.TryGetValue(userId, out var stack) && stack.Count > 0)
        {
            var data = stack.Pop();
            _currDatas[userId] = data;
            return Task.FromResult<Update?>(data);
        }
        else
        {
            return null;
        }
    }

    public Task SetCurrent(long userId, Update update)
    {
        if (!_userHistories.ContainsKey(userId))
            _userHistories[userId] = new Stack<Update>();

        _currDatas.TryGetValue(userId, out var currentData);

        //if (currentData != null && currentData.Split(' ')[0] != update.Split(' ')[0])
        //{
        //    _userHistories[userId].Push(currentData);
        //}

        if (currentData != null)
        {
            _userHistories[userId].Push(currentData);
        }

        _currDatas[userId] = update;

        return Task.CompletedTask;
    }
}
