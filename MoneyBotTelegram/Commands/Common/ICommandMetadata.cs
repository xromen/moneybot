namespace MoneyBotTelegram.Commands.Common;

public interface ICommandMetadata
{
    static abstract CommandMetadata Metadata { get; }
}
