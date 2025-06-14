﻿using MoneyBotTelegram.Services.Models.ProverkaChekov;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MoneyBotTelegram.Services;

public class ProverkaChekaApiService
{
    private HttpClient _httpClient = new() { BaseAddress = new Uri("https://proverkacheka.com") };
    private readonly string _apiKey = Environment.GetEnvironmentVariable("PROVERKA_CHEKA_API_KEY") ?? throw new Exception("Не установлено значение PROVERKA_CHEKA_API_KEY");
    private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    public ProverkaChekaApiService()
    {

    }

    public async Task<GetCheckResponse?> GetCheckDataByPhoto(Stream photoStream, string fileName)
    {
        using var form = new MultipartFormDataContent
        {
            { new StringContent(_apiKey), "token" }
        };

        var streamContent = new StreamContent(photoStream);

        form.Add(streamContent, "qrfile", fileName);

        var response = await _httpClient.PostAsync("/api/v1/check/get", form);

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCheckResponse>(responseContent, _serializerOptions);
    }

    public async Task<GetCheckResponse?> GetCheckDataByQrRaw(string qrRaw)
    {
        var content = JsonContent.Create(new { token = _apiKey, qrraw = qrRaw });

        var response = await _httpClient.PostAsync("/api/v1/check/get", content);

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GetCheckResponse>(responseContent, _serializerOptions);
    }

    
}
