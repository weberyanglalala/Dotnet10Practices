using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using WebApplication1.Controllers.N8n;
using WebApplication1.Common;

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
        // https://devblogs.microsoft.com/semantic-kernel/using-json-schema-for-structured-output-in-net-for-openai-models/
        // Initialize kernel.
        Kernel kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4.1", _configuration["OpenAiApiKey"])
            .Build();

        // Initialize ChatResponseFormat object with JSON schema of desired response format.
        ChatResponseFormat chatResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "travel_itinerary",
            jsonSchema: BinaryData.FromString("""
                {
                    "type": "object",
                    "properties": {
                        "Brand": { "type": "string" },
                        "Name": { "type": "string" },
                        "Category": { "type": "string" },
                        "Description": { "type": "string" },
                        "Region": { "type": "string" },
                        "StartUtc": { "type": "string", "format": "date-time" },
                        "EndUtc": { "type": "string", "format": "date-time" },
                        "Price": { "type": "integer" },
                        "Currency": { "type": "string" }
                    },
                    "required": ["Brand", "Name", "Category", "Description", "Region", "StartUtc", "EndUtc", "Price", "Currency"],
                    "additionalProperties": false
                }
                """),
            jsonSchemaIsStrict: true);

        // Specify response format by setting ChatResponseFormat object in prompt execution settings.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = chatResponseFormat
        };

        // Send a request and pass prompt execution settings with desired response format.
        var result = await kernel.InvokePromptAsync($"你是行銷小編請輸出行程物件\n{request.ProductTitle}，產生相對應的旅遊產品資訊，品牌名稱等，所有資訊為必填", new KernelArguments(executionSettings));

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