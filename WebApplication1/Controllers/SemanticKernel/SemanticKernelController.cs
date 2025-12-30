using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using WebApplication1.Common;
using WebApplication1.Models;

namespace WebApplication1.Controllers.SemanticKernel;

[ApiController]
[Route("api/[controller]/[action]")]
public class SemanticKernelController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public SemanticKernelController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        // Initialize kernel.
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4.1", _configuration["OpenAiApiKey"])
            .Build();

        // Import plugin
        kernel.ImportPluginFromType<TravelItineraryPlugin>();

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = typeof(TravelItinerary), // Specify response format
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() // Enable automatic function calling
        };

        // Send a request and pass prompt execution settings with desired response format.
        var result = await kernel.InvokePromptAsync($"使用旅遊資料為 {request.ProductTitle} 生成旅遊行程", new(executionSettings));

        // Deserialize string response to a strong type to access type properties.
        var itinerary = JsonSerializer.Deserialize<TravelItinerary>(result.ToString());

        var response = new CreateProductResponse
        {
            IsSuccess = true,
            Brand = itinerary.Brand ?? "無品牌名稱",
            Name = itinerary.Name ?? "無產品名稱",
            Category = itinerary.Category ?? "無分類名稱",
            Description = itinerary.Description ?? "無敘述",
            Region = itinerary.Region ?? "無地區",
            StartUtc = itinerary.StartUtc,
            EndUtc = itinerary.EndUtc,
            Price = (decimal)itinerary.Price,
            Currency = itinerary.Currency
        };

        return Ok(new ApiResponse<CreateProductResponse>()
        {
            Data = response,
            Code = 200,
            Message = "成功"
        });
    }

    // Define plugin
    public sealed class TravelItineraryPlugin
    {
        [KernelFunction]
        public List<string> GetTravelData()
        {
            return new List<string>
            {
                "熱門旅遊目的地包括巴黎、東京和紐約。",
                "常見活動包括觀光、購物和用餐。",
                "旅遊產品通常包括旅遊團、酒店和航班。"
            };
        }
    }
}