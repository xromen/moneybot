using MoneyBotTelegram.Infrasctructure;
using System.Transactions;

namespace MoneyBotTelegram.Services;

public class TransactionsService(
    ApplicationDbContext db,
    IUserService userService)
{
    //public async Task<Transaction> GetAllAsync(long userId)
    //{
    //    var users = await userService.GetAllFamilyPersonsAsync(userId);


    //}
}
