using Telegram.Bot.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MoneyBotTelegram.Services;

public interface IUserNavigationService
{
    Task SetCurrent(long userId, NavigationState update);
    Task<NavigationState?> GetCurrent(long userId);
    Task<NavigationState?> PopAsync(long userId);
    Task ClearAsync(long userId);
}
public class NavigationState
{
    public string HandlerName { get; set; }
    public string CommandOrCallback { get; set; } = default!;
    public bool IsMessage { get; set; }
}
public class UserNavigationService : IUserNavigationService
{
    private static Dictionary<long, Stack<NavigationState>> _userHistories = new();
    private static Dictionary<long, NavigationState> _currDatas = new();

    public Task ClearAsync(long userId)
    {
        _userHistories.Remove(userId);
        return Task.CompletedTask;
    }

    public Task<NavigationState?> PopAsync(long userId)
    {
        if (_userHistories.TryGetValue(userId, out var stack) && stack.Count > 0)
        {
            var data = stack.Pop();
            _currDatas[userId] = data;
            return Task.FromResult<NavigationState?>(data);
        }
        else
        {
            return Task.FromResult<NavigationState?>(null);
        }
    }

    public Task SetCurrent(long userId, NavigationState update)
    {
        if (!_userHistories.ContainsKey(userId))
            _userHistories[userId] = new Stack<NavigationState>();

        _currDatas.TryGetValue(userId, out var currentData);

        if (currentData != null)
        {
            _userHistories[userId].Push(currentData);
        }

        _currDatas[userId] = update;

        return Task.CompletedTask;
    }

    public Task<NavigationState?> GetCurrent(long userId)
    {
        _currDatas.TryGetValue(userId, out var currentData);
        return Task.FromResult(currentData);
    }
}
