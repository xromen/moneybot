using Microsoft.EntityFrameworkCore;
using MoneyBotTelegram.Commands.Common;
using MoneyBotTelegram.Infrasctructure;
using MoneyBotTelegram.Infrasctructure.Entities;
using MoneyBotTelegram.Services;
using MoneyBotTelegram.Services.Models.ProverkaChekov;
using SkiaSharp;
using Telegram.Bot;
using Telegram.Bot.Types;
using ZXing.SkiaSharp;

namespace MoneyBotTelegram.Commands;

public enum ImageHandlerState
{
    Idle,
    EnteringCategory
}
public class ImageQRCodeArgs : BaseArgs
{
    public long? CategoryId { get; set; }
    public int? Page { get; set; }
}
public class ImageQRCodeHandler(
    ProverkaChekaApiService api,
    IConversationState<ImageHandlerState> conversation,
    IKeyboardFactory keyboardFactory,
    ApplicationDbContext db,
    ILogger<ImageQRCodeHandler> logger) : BaseCommand<ImageQRCodeArgs>
{
    private static Dictionary<long, GetCheckResponse> _userResponses = new Dictionary<long, GetCheckResponse>();
    public override string Command => "/image_qr_code";

    public override bool CanHandle(Message message)
    {
        return message.Photo != null || conversation.GetState(message.From!.Id) != ImageHandlerState.Idle;
    }

    public override async Task HandleAsync(ITelegramBotClient bot, Message message, CancellationToken cancellationToken, bool editMessage = false)
    {
        var userId = message.From!.Id;

        var args = ParseArgs(message);

        if (args != null)
        {
            await ChooseCategory(bot, message, editMessage);
        }
        else
        {
            await HandlePhotoAsync(bot, message, message.Photo!.Last(), editMessage);
        }

    }

    private async Task HandlePhotoAsync(ITelegramBotClient bot, Message message, PhotoSize photo, bool editMessage)
    {
        var userId = message.From!.Id;
        await using var stream = new MemoryStream();
        var tgFile = await bot.GetInfoAndDownloadFile(photo.FileId, stream);
        stream.Position = 0;

        var statusMessage = await bot.SendMessage(message.Chat.Id, "Отправка изображения на сайт ProverkaChekov...");
        var checkData = await TryGetCheckDataByPhoto(stream, Path.GetFileName(tgFile.FilePath));

        if (checkData.Code == 1)
        {
            await OnSuccessfulCheckData(bot, message, userId, checkData, statusMessage, editMessage);
            return;
        }

        await bot.EditMessageText(statusMessage.Chat.Id, statusMessage.Id, "Ошибка при обработке изображения. Пытаюсь отсканировать QR-код...");

        stream.Position = 0;
        checkData = await TryGetCheckDataByQrCode(bot, stream, statusMessage);

        if (checkData.Code == 1)
            await OnSuccessfulCheckData(bot, message, userId, checkData, statusMessage, editMessage);
    }

    private async Task<GetCheckResponse> TryGetCheckDataByPhoto(Stream stream, string fileName)
    {
        try
        {
            return await api.GetCheckDataByPhoto(stream, fileName) ?? new() { Code = 5 };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при вызове ProverkaChekov по фото");
            return new() { Code = 5 };
        }
    }

    private async Task<GetCheckResponse> TryGetCheckDataByQrCode(ITelegramBotClient bot, Stream stream, Message statusMessage)
    {
        using var bitmap = SKBitmap.Decode(stream);
        var reader = new BarcodeReader { AutoRotate = true, Options = new() { TryInverted = true } };
        var result = reader.Decode(bitmap);

        if (result?.Text is string qrRaw)
        {
            try
            {
                return await api.GetCheckDataByQrRaw(qrRaw) ?? new() { Code = 5 };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при вызове ProverkaChekov по QR raw");
                await bot.EditMessageText(statusMessage.Chat.Id, statusMessage.Id, $"QR-код считан: ({qrRaw}). Ошибка при получении данных");
                return new() { Code = 5 };
            }
        }
        else
        {
            await bot.EditMessageText(statusMessage.Chat.Id, statusMessage.Id, "Не удалось распознать QR-код. Попробуйте еще раз.");
            return new() { Code = 5 };
        }
    }

    private async Task OnSuccessfulCheckData(ITelegramBotClient bot, Message message, long userId, GetCheckResponse checkData, Message statusMessage, bool editMessage)
    {
        _userResponses[userId] = checkData;
        await bot.EditMessageText(statusMessage.Chat.Id, statusMessage.Id, "✅ Данные чека успешно получены");

        var exist = await db.MoneyTransactions.AnyAsync(c => c.CheckQrCodeRaw == checkData.Request.Qrraw);
        if(exist)
        {
            await bot.SendMessage(message.Chat.Id, "Данный чек уже был добавлен ранее");
            return;
        }

        conversation.SetState(userId, ImageHandlerState.EnteringCategory);
        await ChooseCategory(bot, message, editMessage);
    }

    private async Task CreateTransaction(GetCheckResponse checkData, long userId, long categoryId)
    {
        var jsonData = checkData.Data.Json;
        string placeString = jsonData.User;

        var place = await db.Places.FirstOrDefaultAsync(p => p.Name == jsonData.User)
                   ?? await CreateNewPlace(jsonData.User, jsonData.Metadata.Address);

        MoneyTransaction transaction = new()
        {
            Amount = -jsonData.TotalSum / 100,
            Date = DateOnly.FromDateTime(jsonData.DateTime),
            CategoryId = categoryId,
            PlaceId = place.Id,
            UserId = userId,
            CheckQrCodeRaw = checkData.Request.Qrraw,
            Items = jsonData.Items.Select(item => new PurchaseItem
            {
                Name = item.Name,
                Price = item.Price / 100,
                Quantity = item.Quantity
            }).ToList()
        };

        await db.MoneyTransactions.AddAsync(transaction);
        await db.SaveChangesAsync();
    }

    private async Task<Place> CreateNewPlace(string name, string address)
    {
        var place = new Place { Name = name, Description = address };
        await db.Places.AddAsync(place);
        await db.SaveChangesAsync();
        return place;
    }

    private async Task ChooseCategory(ITelegramBotClient bot, Message message, bool editMessage)
    {
        var keyboard = keyboardFactory.Empty();
        string responseText = string.Empty;
        var userId = message.From!.Id;

        var args = ParseArgs(message);

        if (args != null && args.CategoryId.HasValue)
        {
            conversation.SetState(userId, ImageHandlerState.Idle);

            var checkData = _userResponses[userId];
            await CreateTransaction(checkData, userId, args.CategoryId.Value);

            responseText = "✅ Транзакция успешно добавлена";
        }
        else
        {
            var categories = await db.Categories.ToListAsync();

            keyboard.AddPaginated(
                categories,
                (i) => i.Name,
                (i) => BuildArgs(new() { CategoryId = i.Id }),
                (p) => BuildArgs(new() { Page = p }),
                args == null || args.Page == null ? 1 : args.Page.Value,
                5);

            responseText += "📋 Выберите категорию:";
        }

        await SendOrEditMessage(bot, message, responseText, keyboard.Create(), editMessage);
    }
}
