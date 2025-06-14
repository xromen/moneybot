using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Infrasctructure.Entities;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = MoneyBotTelegram.Infrasctructure.Entities.User;

namespace MoneyBotTelegram.Services;

public interface IUserService
{
    Task<User?> GetAsync(long id, CancellationToken cancellationToken = default);
    Task<User?> GetAsync(string userName, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetYourFamilyAsync(long userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetAllFamilyPersonsAsync(long userId, CancellationToken cancellationToken = default);
    Task SaveAsync(User entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task SendMessageAsync(long userId, string message, CancellationToken cancellationToken = default);
}

public class UserService(
    ApplicationDbContext context,
    ITelegramBotClient botClient,
    ILogger<UserService> logger) : IUserService
{
    public Task<User?> GetAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.Users.Include(c => c.Family).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public Task<User?> GetAsync(string userName, CancellationToken cancellationToken = default)
    {
        return context.Users.Include(c => c.Family).FirstOrDefaultAsync(c => c.Username.Equals(userName), cancellationToken);
    }

    public async Task<IEnumerable<User>> GetYourFamilyAsync(long userId, CancellationToken cancellationToken = default)
    {
        return await context.Users.Include(c => c.Family).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllFamilyPersonsAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync(userId, cancellationToken);

        
        return await GetYourFamilyAsync(userId, cancellationToken);
    }

    public async Task SaveAsync(User entity, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(entity.Id, cancellationToken);

        if (user == null)
        {
            await context.Users.AddAsync(entity, cancellationToken);
        }
        else
        {
            context.Users.Update(user);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var user = await GetAsync(id, cancellationToken);

        if (user == null)
        {
            return false;
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public Task SendMessageAsync(long userId, string message, CancellationToken cancellationToken = default)
    {
        return botClient.SendMessage(userId,
                    message,
                    cancellationToken: cancellationToken);
    }
}
