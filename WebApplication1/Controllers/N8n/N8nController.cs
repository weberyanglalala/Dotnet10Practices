using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Common;

namespace WebApplication1.Controllers.N8n;

[ApiController]
[Route("api/[controller]/[action]")]
public class N8NController : ControllerBase
{
    private readonly ILogger<N8NController> _logger;
    private readonly IConfiguration _configuration;
    public N8NController(ILogger<N8NController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // create a httpclient
        var client = new HttpClient();
        // set up request endpoint
        var endpoint = _configuration["N8nWebhookEndpoint"];
        // setup headers
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _configuration["N8nApiKey"]);
        // send json to the endpoint using http method post 
        var response = await client.PostAsJsonAsync(endpoint, request);
        // get response content
        var responseString = await response.Content.ReadAsStringAsync();
        // deserialize CreateProductResponse
        var result = JsonSerializer.Deserialize<CreateProductResponse>(responseString);
        // return Ok(result);
        return Ok(new ApiResponse<CreateProductResponse>
        {
            Data = result,
            Code = 200,
            Message = "取得商品資料成功"
        });
    }
}

public class CreateProductRequest
{
    [JsonPropertyName("productTitle")]
    public string ProductTitle { get; set; }
}

public class CreateProductResponse
{
    [JsonPropertyName("isSuccess")] public bool IsSuccess { get; set; }
    [JsonPropertyName("brand")] public string Brand { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
    [JsonPropertyName("category")] public string Category { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("region")] public string Region { get; set; }
    [JsonPropertyName("startUtc")] public DateTime StartUtc { get; set; }
    [JsonPropertyName("endUtc")] public DateTime EndUtc { get; set; }
    [JsonPropertyName("price")] public decimal Price { get; set; }
    [JsonPropertyName("currency")] public string Currency { get; set; }
}