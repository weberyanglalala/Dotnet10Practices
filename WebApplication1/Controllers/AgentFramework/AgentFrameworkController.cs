using Microsoft.AspNetCore.Mvc;
using OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Agents.AI;
using System.Text.Json;
using WebApplication1.Controllers.N8n;
using WebApplication1.Common;

namespace WebApplication1.Controllers.AgentFramework;

public class TravelItinerary
{
    public string Brand { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Region { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int Price { get; set; }
    public string Currency { get; set; }
}

[ApiController]
[Route("api/[controller]/[action]")]
public class AgentFrameworkController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AgentFrameworkController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(typeof(TravelItinerary));

        ChatOptions chatOptions = new()
        {
            ResponseFormat = ChatResponseFormat.ForJsonSchema(
                schema: schema,
                schemaName: "TravelItinerary",
                schemaDescription: "行程資訊"
            )
        };

        var client = new OpenAIClient(_configuration["OpenAiApiKey"]).GetChatClient("gpt-4.1");

        var chatClient = client.AsIChatClient();

        var agent = chatClient.CreateAIAgent(new ChatClientAgentOptions()
        {
            Name = "TravelAgent",
            Description = "請輸出行程物件",
            ChatOptions = chatOptions
        });

        var response = await agent.RunAsync(request.ProductTitle);

        var itinerary = response.Deserialize<TravelItinerary>(JsonSerializerOptions.Web);

        var createResponse = new CreateProductResponse
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
            Data = createResponse,
            Code = 200,
            Message = "成功"
        });
    }
}