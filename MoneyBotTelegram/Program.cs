using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using Telegram.Bot;

namespace MoneyBotTelegram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
            .UseSnakeCaseNamingConvention());

            builder.Services.AddScoped(provider => provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            builder.Services.AddSingleton(typeof(IEntityCacheService<>), typeof(EntityCacheService<>));

            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(builder.Configuration.GetValue<string>("BotConfiguration:Token")!));

            builder.Services.AddScoped(typeof(IDraftService<>), typeof(DraftService<>));
            builder.Services.AddScoped(typeof(IConversationState<>), typeof(ConversationState<>));
            builder.Services.AddScoped<IUserNavigationService, UserNavigationService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddBotCallbackQueryHandlers(typeof(Program).Assembly)
                .AddScoped<CallbackQueryRouter>();

            builder.Services.AddBotCommandHandlers(typeof(Program).Assembly)
                .AddScoped<CommandRouter>();

            builder.Services.AddScoped<IKeyboardFactory, KeyboardFactory>();

            builder.Services.AddSingleton<BotUpdateHandler>();
            builder.Services.AddHostedService<BotBackgroundService>();

            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
                };
            });

            //builder.Services.AddExceptionHandler<ExceptionHandler>();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
