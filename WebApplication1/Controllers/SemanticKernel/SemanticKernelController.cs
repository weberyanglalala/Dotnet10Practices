using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using WebApplication1.Controllers.N8n;
using WebApplication1.Common;

namespace WebApplication1.Controllers.SemanticKernel;

[ApiController]
[Route("api/[controller]/[action]")]
public class SemanticKernelController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", "")
            .Build();

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = typeof(TravelItinerary)
        };

        var result = await kernel.InvokePromptAsync($"請輸出行程物件\n{request.ProductTitle}", new KernelArguments(executionSettings));

        var itinerary = JsonSerializer.Deserialize<TravelItinerary>(result.ToString());

        var response = new CreateProductResponse
        {
            IsSuccess = true,
            Brand = itinerary.Brand,
            Name = itinerary.Name,
            Category = itinerary.Category,
            Description = itinerary.Description,
            Region = itinerary.Region,
            StartUtc = DateTime.Parse(itinerary.StartUtc),
            EndUtc = DateTime.Parse(itinerary.EndUtc),
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
    public string Brand { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Region { get; set; }
    public string StartUtc { get; set; }
    public string EndUtc { get; set; }
    public int Price { get; set; }
    public string Currency { get; set; }
}