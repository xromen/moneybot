namespace MoneyBotTelegram.Services;

public interface IDraftService<T>
{
    T GetDraft(long userId);
    void UpdateDraft(long userId, Action<T> update);
    void ClearDraft(long userId);
}
public class DraftService<T> : IDraftService<T> where T : new()
{
    private static Dictionary<long, T> _drafts = new Dictionary<long, T>();

    public void ClearDraft(long userId) => _drafts.Remove(userId);

    public T GetDraft(long userId)
    {
        if(_drafts.TryGetValue(userId, out var draft))
        {
            return draft;
        }
        else
        {
            draft = new T();
            _drafts.Add(userId, draft);
            return draft;
        }
    }

    public void UpdateDraft(long userId, Action<T> update)
    {
        var draft = GetDraft(userId);
        update(draft);
    }
}
