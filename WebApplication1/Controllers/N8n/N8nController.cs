using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Common;
using WebApplication1.Models;

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