using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.CallbackQueries.Common;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Services;
using System.Globalization;
using Telegram.Bot;

namespace MoneyBotTelegram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo russianCulture = new CultureInfo("ru-RU");

            // Устанавливаем культуру по умолчанию для всех потоков
            CultureInfo.DefaultThreadCurrentCulture = russianCulture;
            CultureInfo.DefaultThreadCurrentUICulture = russianCulture;

            var apiKey = Environment.GetEnvironmentVariable("TG_KEY");
            var pgCs = Environment.GetEnvironmentVariable("PG_CS");

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(pgCs))
            {
                throw new Exception("Not set environment variables");
            }

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<ProverkaChekaApiService>();

            builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
                options.UseNpgsql(pgCs)
            .UseSnakeCaseNamingConvention());

            builder.Services.AddScoped(provider => provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

            builder.Services.AddSingleton(typeof(IEntityCacheService<>), typeof(EntityCacheService<>));

            builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(apiKey));

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

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    dbContext.Database.Migrate();
                }
                catch (Exception ex)
                {
                    throw new("migration fail", ex);
                }
            }

            app.Run();
        }
    }
}
