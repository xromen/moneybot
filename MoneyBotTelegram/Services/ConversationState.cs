using System.Transactions;

namespace MoneyBotTelegram.Services;

public interface IConversationState<T>
{
    T GetState(long userId);
    void SetState(long userId, T state);
    void ClearState(long userId);
}

public class ConversationState<T> : IConversationState<T> where T : Enum
{
    private static Dictionary<long, T> _states = new();
    public void ClearState(long userId) => _states.Remove(userId);

    public T? GetState(long userId) => _states.TryGetValue(userId, out T state) ? state : default;

    public void SetState(long userId, T state) => _states[userId] = state;
}
