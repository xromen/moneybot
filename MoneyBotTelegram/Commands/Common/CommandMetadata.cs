namespace MoneyBotTelegram.Commands.Common;

public record CommandMetadata(string Command, string Description, int Order = 10);
