//using MoneyBotTelegram.Commands.Common;

//namespace MoneyBotTelegram.Services;

//public interface IUserStateStore
//{
//    void SetUserState(IBotCommandHandler command, long userId, UserState userState);
//    void ClearUserState(long userId);
//    UserCommandState? GetUserState(long userId);
//}

//public class UserStateStore : IUserStateStore
//{
//    private static Dictionary<long, UserCommandState> _userCommandStates = new();

//    public void SetUserState(IBotCommandHandler command, long userId, UserState userState)
//    {
//        _userCommandStates[userId] = new(command, userState);
//    }

//    public void ClearUserState(long userId)
//    {
//        _userCommandStates.Remove(userId);
//    }

//    public UserCommandState? GetUserState(long userId)
//    {
//        if (_userCommandStates.TryGetValue(userId, out var userCommandState))
//        {
//            return userCommandState;
//        }
//        else
//        {
//            return null;
//        }
//    }
//}
//public record UserCommandState(IBotCommandHandler Command, UserState UserState);
