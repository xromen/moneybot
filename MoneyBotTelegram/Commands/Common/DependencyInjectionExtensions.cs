using System.Reflection;

namespace MoneyBotTelegram.Commands.Common;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddBotCommandHandlers(this IServiceCollection services, Assembly assembly)
    {
        List<CommandMetadata> addedCommands = new();

        var handlerTypes = assembly.GetTypes()
            .Where(x => x is { IsClass: true, IsAbstract: false } && x.IsAssignableTo(typeof(IBotCommandHandler)));

        foreach (var type in handlerTypes)
        {
            services.AddScoped(typeof(IBotCommandHandler), type);

            var metadataInterface = type.GetInterfaces()
                .FirstOrDefault(x => x == typeof(ICommandMetadata));

            if (metadataInterface == null)
            {
                continue;
            }

            var property = type.GetProperty(nameof(ICommandMetadata.Metadata), BindingFlags.Public | BindingFlags.Static);

            if (property?.GetValue(null) is CommandMetadata metadata)
            {
                if(addedCommands.Any(c => c.Command == metadata.Command))
                {
                    throw new Exception($"Команда {metadata.Command} уже существует");
                }

                services.AddSingleton(metadata);
                addedCommands.Add(metadata);
            }
        }

        return services;
    }

    public static TCommand? GetCommandHandler<TCommand>(this IServiceProvider serviceProvider)
    {
        var handlers = serviceProvider.GetRequiredService<IEnumerable<IBotCommandHandler>>();

        return (TCommand)handlers.FirstOrDefault(c => c.GetType() == typeof(TCommand));
    }
}
