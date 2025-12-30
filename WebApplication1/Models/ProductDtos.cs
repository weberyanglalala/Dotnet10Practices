using System.Text.Json.Serialization;

namespace WebApplication1.Models;

public class CreateProductRequest
{
    [JsonPropertyName("productTitle")]
    public string ProductTitle { get; set; }
}

public class CreateProductResponse
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; }

    [JsonPropertyName("brand")]
    public string Brand { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("region")]
    public string Region { get; set; }

    [JsonPropertyName("startUtc")]
    public DateTime StartUtc { get; set; }

    [JsonPropertyName("endUtc")]
    public DateTime EndUtc { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }
}

public class TravelItinerary
{
    public required string Brand { get; set; }
    public required string Name { get; set; }
    public required string Category { get; set; }
    public required string Description { get; set; }
    public required string Region { get; set; }
    public required DateTime StartUtc { get; set; }
    public required DateTime EndUtc { get; set; }
    public required int Price { get; set; }
    public required string Currency { get; set; }
}
