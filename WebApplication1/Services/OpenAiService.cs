using System.Text.Json;
using OpenAI.Chat;
using WebApplication1.Common;
using WebApplication1.Models;

namespace WebApplication1.Services;

/// <summary>
/// 代表與 OpenAI 平台整合的服務，用於提供產品相關的推薦和操作。
/// 實作 <c>IProductDetailRecommendationService</c> 介面。
/// 參考來源: https://github.com/openai/openai-dotnet/blob/main/examples/Chat/Example06_StructuredOutputsAsync.cs
/// </summary>
public class OpenAiService : IProductDetailRecommendationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OpenAiService> _logger;

    public OpenAiService(IConfiguration configuration, ILogger<OpenAiService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            // 1. 驗證配置
            var apiKey = _configuration["OpenAiApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("OpenAI API Key 未設定");
                return OperationResult<CreateProductResponse>.Failure("OpenAI API Key 未設定", 500);
            }

            // 2. 建立 ChatClient
            ChatClient client = new("gpt-4.1", apiKey);

            // 3. 準備訊息
            List<ChatMessage> messages =
            [
                new SystemChatMessage("請輸出行程物件"),
                new UserChatMessage(request.ProductTitle)
            ];

            // 4. 設定 ChatCompletionOptions
            ChatCompletionOptions options = new()
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                    jsonSchemaFormatName: "travel_itinerary",
                    jsonSchema: GetTravelItinerarySchema(),
                    jsonSchemaIsStrict: true
                ),
            };

            // 5. 呼叫 OpenAI API
            ChatCompletion completion = await client.CompleteChatAsync(messages, options);

            // 6. 解析 JSON 回應
            using JsonDocument json = JsonDocument.Parse(completion.Content[0].Text);
            JsonElement root = json.RootElement;

            // 7. 建立回應物件
            var response = new CreateProductResponse
            {
                IsSuccess = true,
                Brand = root.GetProperty("brand").GetString(),
                Name = root.GetProperty("name").GetString(),
                Category = root.GetProperty("category").GetString(),
                Description = root.GetProperty("description").GetString(),
                Region = root.GetProperty("region").GetString(),
                StartUtc = DateTime.Parse(root.GetProperty("startUtc").GetString()!),
                EndUtc = DateTime.Parse(root.GetProperty("endUtc").GetString()!),
                Price = (decimal)root.GetProperty("price").GetInt32(),
                Currency = root.GetProperty("currency").GetString()
            };

            _logger.LogInformation("成功建立產品: {ProductName}", response.Name);
            return OperationResult<CreateProductResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立產品時發生未預期的錯誤");
            return OperationResult<CreateProductResponse>.Failure("內部伺服器錯誤", 500);
        }
    }

    private static BinaryData GetTravelItinerarySchema()
    {
        return BinaryData.FromBytes("""
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
        """u8.ToArray());
    }
}
