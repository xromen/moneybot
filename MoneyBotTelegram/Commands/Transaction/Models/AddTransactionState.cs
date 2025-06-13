namespace MoneyBotTelegram.Commands.Transaction.Models;

public enum AddTransactionState
{
    Idle = 0,
    EnteringAmount = 1,
    EnteringDescription = 2,
    EnteringCategory = 3,
    EnteringDate = 4,
    EnteringPlace = 5,
    Clear = 6,
    Save = 7
}
