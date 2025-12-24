using System.Net;
using System.Net.Http.Headers;
using System.Text;
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
    public N8NController(ILogger<N8NController> logger)
    {
        _logger = logger;
    }
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var client = new HttpClient();
        // set up request endpoint
        var endpoint = "";

        // setup headers
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "");

        // set up request content
        var jsonContent = JsonSerializer.Serialize(request);
        
        var response = await client.PostAsJsonAsync(
            endpoint,
            jsonContent
        );

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("n8n error: {Status}, Reason: {Reason}, Content: {Content}", 
                response.StatusCode, 
                response.ReasonPhrase, 
                errorContent);
            return StatusCode((int)response.StatusCode, "呼叫 n8n 失敗");
        }

        var data = await response.Content
            .ReadFromJsonAsync<CreateProductResponse>();

        if (data == null)
        {
            return Problem("n8n 回傳格式錯誤");
        }

        return Ok(new ApiResponse<CreateProductResponse>
        {
            Data = data,
            Code = 200,
            Message = "取得商品資料成功"
        });
    }
}

public class CreateProductRequest
{
    [JsonPropertyName("productTitle")] public string ProductTitle { get; set; }
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