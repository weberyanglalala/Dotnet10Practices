using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.Text.Json;
using WebApplication1.Controllers.N8n;
using WebApplication1.Common;

namespace WebApplication1.Controllers.OpenAi;

[ApiController]
[Route("api/[controller]/[action]")]
public class OpenAiController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public OpenAiController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        ChatClient client = new(
            "gpt-4.1",
            _configuration["OpenAiApiKey"]
        );

        List<ChatMessage> messages =
        [
            new SystemChatMessage("請輸出行程物件"),
            new UserChatMessage(request.ProductTitle)
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "travel_itinerary",
                jsonSchema: BinaryData.FromBytes("""
                {
                  "type": "object",
                  "properties": {
                    "brand": {
                      "type": "string",
                      "description": "旅遊品牌名稱"
                    },
                    "name": {
                      "type": "string",
                      "description": "形如 '品牌名稱｜景點/描述五日遊'；可包含區域景點與特色"
                    },
                    "category": {
                      "type": "string",
                      "enum": [
                        "水上活動",
                        "山地健行",
                        "溫泉",
                        "博物館與美術館",
                        "老街與市集",
                        "海島與海岸線",
                        "自然風景與國家公園",
                        "文化古蹟與歷史景點",
                        "美食巡禮",
                        "購物與手作工藝"
                      ]
                    },
                    "description": {
                      "type": "string",
                      "description": "行程亮點、節奏、適合人群等詳述"
                    },
                    "region": {
                      "type": "string",
                      "enum": [
                        "臺北市",
                        "新北市",
                        "桃園市",
                        "臺中市",
                        "臺南市",
                        "高雄市",
                        "基隆市",
                        "新竹市",
                        "新竹縣",
                        "彰化縣",
                        "南投縣",
                        "雲林縣",
                        "嘉義市",
                        "嘉義縣",
                        "屏東縣",
                        "宜蘭縣",
                        "花蓮縣",
                        "臺東縣",
                        "澎湖縣",
                        "金門縣",
                        "連江縣"
                      ]
                    },
                    "startUtc": {
                      "type": "string",
                      "format": "date-time"
                    },
                    "endUtc": {
                      "type": "string",
                      "format": "date-time"
                    },
                    "price": {
                      "type": "integer"
                    },
                    "currency": {
                      "type": "string",
                      "enum": ["TWD"]
                    }
                  },
                  "required": [
                    "brand",
                    "name",
                    "category",
                    "description",
                    "region",
                    "startUtc",
                    "endUtc",
                    "price",
                    "currency"
                  ],
                  "additionalProperties": false
                }
                """u8.ToArray()),
                jsonSchemaIsStrict: true
            )
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);

        using JsonDocument json = JsonDocument.Parse(completion.Content[0].Text);
        JsonElement root = json.RootElement;

        var response = new CreateProductResponse
        {
            IsSuccess = true,
            Brand = root.GetProperty("brand").GetString(),
            Name = root.GetProperty("name").GetString(),
            Category = root.GetProperty("category").GetString(),
            Description = root.GetProperty("description").GetString(),
            Region = root.GetProperty("region").GetString(),
            StartUtc = DateTime.Parse(root.GetProperty("startUtc").GetString()),
            EndUtc = DateTime.Parse(root.GetProperty("endUtc").GetString()),
            Price = (decimal)root.GetProperty("price").GetInt32(),
            Currency = root.GetProperty("currency").GetString()
        };

        return Ok(new ApiResponse<CreateProductResponse>()
        {
            Data = response,
            Code = 200,
            Message = "成功"
        });
    }
}