using System.Net.Http.Json;
using System.Text.Json.Serialization;
using YugiohDeck.API.Models;
using YugiohDeck.API.Models.Dto;

namespace YugiohDeck.API.Services;

public class YgoApiService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://db.ygoprodeck.com/api/v7/cardinfo.php";

    public YgoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Faz a busca fuzzy na API externa para o Front-end listar cartas.
    /// </summary>
    public async Task<List<CardDto>> SearchCardsAsync(string query, string language = "pt")
    {
        var url = $"{BaseUrl}?fname={Uri.EscapeDataString(query)}&language={language}";
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse>(url);
            if (response?.Data == null) return new List<CardDto>();

            return response.Data.Select(apiCard => new CardDto
            {
                Id = apiCard.Id,
                Name = apiCard.Name,
                Type = apiCard.Type,
                FrameType = apiCard.FrameType,
                Desc = apiCard.Desc,
                Race = apiCard.Race,
                Attribute = apiCard.Attribute,
                Atk = apiCard.Atk,
                Def = apiCard.Def,
                Level = apiCard.Level,
                ImageUrl = apiCard.CardImages.FirstOrDefault()?.ImageUrl,
                LastUpdate = DateTime.UtcNow
            }).ToList();
        }
        catch (HttpRequestException)
        {
            return new List<CardDto>();
        }
    }

    /// <summary>
    /// Busca uma carta específica por ID para salvar no banco de dados local.
    /// </summary>
    public async Task<Card?> GetCardByIdAsync(int cardId, string language = "pt")
    {
        var url = $"{BaseUrl}?id={cardId}&language={language}";
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse>(url);
            var apiCard = response?.Data?.FirstOrDefault();
            
            if (apiCard == null) return null;

            return new Card
            {
                Id = apiCard.Id,
                Name = apiCard.Name,
                Type = apiCard.Type,
                FrameType = apiCard.FrameType,
                Desc = apiCard.Desc,
                Race = apiCard.Race,
                Attribute = apiCard.Attribute,
                Atk = apiCard.Atk,
                Def = apiCard.Def,
                Level = apiCard.Level,
                ImageUrl = apiCard.CardImages.FirstOrDefault()?.ImageUrl,
                LastUpdate = DateTime.UtcNow
            };
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    // =========================================================================
    // Modelos Privados para Desserialização do JSON (Evita sujar a pasta Models)
    // =========================================================================
    private record ApiResponse
    {
        [JsonPropertyName("data")]
        public List<ApiCardData> Data { get; init; } = new();
    }

    private record ApiCardData
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
        [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
        [JsonPropertyName("frameType")] public string? FrameType { get; init; }
        [JsonPropertyName("desc")] public string? Desc { get; init; }
        [JsonPropertyName("race")] public string? Race { get; init; }
        [JsonPropertyName("attribute")] public string? Attribute { get; init; }
        [JsonPropertyName("atk")] public int? Atk { get; init; }
        [JsonPropertyName("def")] public int? Def { get; init; }
        [JsonPropertyName("level")] public int? Level { get; init; }
        [JsonPropertyName("card_images")] public List<ApiCardImage> CardImages { get; init; } = new();
    }

    private record ApiCardImage
    {
        [JsonPropertyName("image_url")] public string ImageUrl { get; init; } = string.Empty;
    }
}