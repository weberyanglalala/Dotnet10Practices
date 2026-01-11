using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using WebApplication1.Common;
using WebApplication1.Models;

namespace WebApplication1.Controllers.ProductSearch;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProductSearchController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly Kernel _kernel;
    private readonly ILogger<ProductSearchController> _logger;

    public ProductSearchController(IConfiguration configuration, Kernel kernel, ILogger<ProductSearchController> logger)
    {
        _configuration = configuration;
        _kernel = kernel;
        _logger = logger;
    }

    /// <summary>
    /// 建立 Qdrant 集合
    /// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/?pivots=programming-language-csharp
    /// </summary>
    /// <returns>回傳建立的集合物件</returns>
    /// <exception cref="ArgumentNullException">當 Qdrant 設定遺失時拋出</exception>
    public async Task<IActionResult> CreateCollection()
    {
        // 建立 Qdrant 用戶端
        var qdrantClient = new QdrantClient(
            host: _configuration["Qdrant:Host"] ?? throw new ArgumentNullException("Qdrant:Host"),
            apiKey: _configuration["Qdrant:ApiKey"] ?? throw new ArgumentNullException("Qdrant:ApiKey"),
            https: true
        );
        // 建立 Qdrant VectorStore 物件
        var vectorStore =
            new QdrantVectorStore(qdrantClient, ownsClient: true);
        // 從資料庫中選擇一個集合,並透過泛型參數指定儲存在其中的鍵和記錄的類型
        var collection = vectorStore.GetCollection<ulong, Hotel>("hotels");
        await collection.EnsureCollectionExistsAsync();
        return Ok(collection);
    }

    /// <summary>
    /// 初始化飯店集合
    /// 建立 Qdrant 向量存放區並將飯店資料上傳至集合中
    /// </summary>
    /// <returns>回傳上傳結果的 API 回應</returns>
    /// <exception cref="ArgumentNullException">當 Qdrant 設定遺失時拋出</exception>
    public async Task<IActionResult> InitializeHotelCollection()
    {
        // 建立 Qdrant 用戶端
        var qdrantClient = new QdrantClient(
            host: _configuration["Qdrant:Host"] ?? throw new ArgumentNullException("Qdrant:Host"),
            apiKey: _configuration["Qdrant:ApiKey"] ?? throw new ArgumentNullException("Qdrant:ApiKey"),
            https: true
        );
        // 建立 Qdrant VectorStore 物件
        var vectorStore =
            new QdrantVectorStore(qdrantClient, ownsClient: true);
        // 從資料庫中選擇一個集合,並透過泛型參數指定儲存在其中的鍵和記錄的類型
        var collection = vectorStore.GetCollection<ulong, Hotel>("hotels");
        var hotels = new List<Hotel>
        {
            new Hotel
            {
                HotelId = 1,
                HotelName = "大皇宮酒店",
                Description = "位於市中心的一家豪華酒店，提供壯麗景色和世界級設施。享受寬敞的客房、美食餐廳和屋頂泳池。適合商務和休閒旅客。",
            },
            new Hotel
            {
                HotelId = 2,
                HotelName = "日落大道度假村",
                Description = "坐落在海灘旁，這家度假村提供寧靜的避風港，享有美麗的日落和一流服務。賓客可享受水上運動、spa護理和高級餐飲。適合浪漫之旅。",
            },
            new Hotel
            {
                HotelId = 3,
                HotelName = "山景旅館",
                Description = "一座被壯觀山脈環繞的溫馨旅館，提供寧靜的環境和戶外活動。享受遠足小徑、溫暖壁爐和鄉村魅力。適合自然愛好者。",
            },
            new Hotel
            {
                HotelId = 4,
                HotelName = "市中心酒店",
                Description = "位於繁華的市中心，這家酒店提供現代化客房並可輕鬆前往熱門景點。享受購物、餐飲和門口的娛樂活動。適合城市探險者。",
            },
            new Hotel
            {
                HotelId = 5,
                HotelName = "海洋微風酒店",
                Description = "這家海濱酒店擁有寬敞的客房和多樣的水上運動。放鬆在泳池旁，享用海景餐飲，感受海洋微風。適合熱帶度假。",
            },
            new Hotel
            {
                HotelId = 6,
                HotelName = "皇家花園",
                Description = "一座擁有典雅裝飾和茂盛花園的宏偉酒店，為賓客提供皇室般的體驗。享受下午茶、豪華套房和無可挑剔的服務。適合特殊場合。",
            },
            new Hotel
            {
                HotelId = 7,
                HotelName = "湖畔休憩處",
                Description = "一座位於湖邊的迷人休憩處，提供寧靜的環境和舒適的住宿。賓客可享受釣魚、划船和湖邊野餐。適合週末度假。",
            },
            new Hotel
            {
                HotelId = 8,
                HotelName = "歷史市中心旅館",
                Description = "位於一座歷史建築內，這家旅館將舊世界魅力與現代舒適相結合。探索附近的博物館，享用復古餐廳美食，欣賞獨特建築。適合歷史愛好者。",
            },
            new Hotel
            {
                HotelId = 9,
                HotelName = "天際套房",
                Description = "每間套房均享有城市全景，這家酒店適合喜愛城市景觀的人士。享受豪華設施、屋頂酒吧和靠近景點的便利。豪華且現代。",
            },
            new Hotel
            {
                HotelId = 10,
                HotelName = "綠谷小屋",
                Description = "坐落於風景如畫的山谷中，這家小屋提供寧靜放鬆的環境。賓客可探索遠足小徑，享受溫暖壁爐，放鬆在大自然中。適合充滿自然風情的假期。",
            },
            new Hotel
            {
                HotelId = 11,
                HotelName = "日出酒店",
                Description = "從這家酒店的陽台房間體驗令人驚嘆的日出。賓客可享受現代化設施、健身中心和免費早餐。提供現代便利和優質服務。",
            },
            new Hotel
            {
                HotelId = 12,
                HotelName = "河畔酒店",
                Description = "位於河邊，這家酒店提供美麗的景色和寧靜的氛圍。賓客可享受河邊餐飲、遊船和風景優美的散步。適合放鬆住宿。",
            },
            new Hotel
            {
                HotelId = 13,
                HotelName = "海濱度假村",
                Description = "一家位於海邊的豪華度假村，提供多種娛樂活動和高級餐飲。享受海灘通道、水上運動和美食。適合家庭和情侶。",
            },
            new Hotel
            {
                HotelId = 14,
                HotelName = "頂峰酒店",
                Description = "位於城市最高點，這家酒店提供壯麗景色和一流設施。賓客可享受屋頂泳池、高級餐飲和豪華套房。適合豪華住宿。",
            },
            new Hotel
            {
                HotelId = 15,
                HotelName = "城市綠洲酒店",
                Description = "位於城市中的一片綠洲，提供放鬆環境和現代設施。賓客可在spa放鬆，享受屋頂花園，時尚用餐。適合商務和休閒。",
            },
            new Hotel
            {
                HotelId = 16,
                HotelName = "復古旅館",
                Description = "一家擁有復古裝飾和溫馨氛圍的迷人旅館。享受古董傢俱、溫暖壁爐和個性化服務。適合欣賞經典風格和舒適的人士。",
            },
            new Hotel
            {
                HotelId = 17,
                HotelName = "海濱天堂",
                Description = "一家擁有壯麗海景和豪華設施的海濱酒店。賓客可在私人海灘放鬆，享受水上運動，並觀景用餐。適合熱帶度假。",
            },
            new Hotel
            {
                HotelId = 18,
                HotelName = "宏偉酒店",
                Description = "體驗這家宏偉酒店的奢華，提供壯麗建築和世界級服務。享受典雅套房、豪華spa和高級餐飲。適合奢華住宿。",
            },
            new Hotel
            {
                HotelId = 19,
                HotelName = "森林避風港",
                Description = "位於茂密森林中的寧靜休憩處，提供和平與安寧。賓客可探索自然小徑，壁爐旁放鬆，享受鄉村魅力。適合自然愛好者。",
            },
            new Hotel
            {
                HotelId = 20,
                HotelName = "沙漠幻景酒店",
                Description = "位於沙漠中的一家獨特酒店，提供壯麗景色和豪華住宿。享受沙漠之旅、清涼泳池和高級餐飲。適合異國情調的逃逸。",
            },
            new Hotel
            {
                HotelId = 21,
                HotelName = "港口酒店",
                Description = "位於港口旁，這家酒店提供美麗的水邊景色和現代設施。賓客可享受遊船、海鮮餐飲和風景散步。適合放鬆住宿。",
            },
            new Hotel
            {
                HotelId = 22,
                HotelName = "山間小屋",
                Description = "一座位於山中的鄉村小屋，提供舒適住宿和戶外冒險。享受遠足、釣魚和溫暖壁爐。適合自然逃逸。",
            },
            new Hotel
            {
                HotelId = 23,
                HotelName = "城市燈光酒店",
                Description = "位於市中心，這家酒店提供現代化客房並可輕鬆前往夜生活場所。享受時尚酒吧、美食餐廳和充滿活力的娛樂。適合城市探險者。",
            },
            new Hotel
            {
                HotelId = 24,
                HotelName = "花園旅館",
                Description = "一家擁有美麗花園和寧靜氛圍的迷人旅館。賓客可在花園放鬆，享受下午茶，舒適房間內休息。適合放鬆度假。",
            },
            new Hotel
            {
                HotelId = 25,
                HotelName = "海景度假村",
                Description = "一家擁有壯麗海景和一流設施的豪華度假村。賓客可在海灘放鬆，享受水上運動，觀景用餐。適合海灘假期。",
            },
            new Hotel
            {
                HotelId = 26,
                HotelName = "皇家套房",
                Description = "體驗這家酒店的奢華，提供寬敞套房和世界級服務。享受典雅裝飾、豪華spa和高級餐飲。適合奢華住宿。",
            },
            new Hotel
            {
                HotelId = 27,
                HotelName = "湖畔酒店",
                Description = "位於湖邊，這家酒店提供寧靜環境和現代舒適。賓客可享受釣魚、划船和湖邊野餐。適合寧靜休憩。"
            },
            new Hotel
            {
                HotelId = 28,
                HotelName = "城市酒店",
                Description = "一家位於市中心的現代酒店，提供時尚客房和優質服務。享受現代設施、健身中心和靠近景點的便利。適合商務和休閒。"
            },
            new Hotel
            {
                HotelId = 29,
                HotelName = "日落旅館",
                Description = "從這家溫馨旅館欣賞美麗的日落，提供舒適住宿和放鬆氛圍。賓客可在泳池旁放鬆，露台上用餐，享受風景。適合寧靜度假。"
            },
            new Hotel
            {
                HotelId = 30,
                HotelName = "大度假村",
                Description = "一家提供豪華設施和壯麗景色的宏偉度假村。享受寬敞客房、spa和高級餐飲。適合難忘的假期。"
            }
        };

        foreach (var hotel in hotels)
        {
            try
            {
                // 為當前飯店 Description 生成並設定 DescriptionEmbedding
                hotel.DescriptionEmbedding = await GenerateEmbeddingAsync(hotel.Description);

                // 將飯店資料更新插入至集合中
                await collection.UpsertAsync(hotel);
                _logger.LogInformation(
                    $"飯店 ID: {hotel.HotelId}, 飯店名稱: {hotel.HotelName} - 成功上傳");
            }
            catch (Exception ex)
            {
                _logger.LogError($"處理飯店 {hotel.HotelId} ({hotel.HotelName}) 時發生錯誤: {ex.Message}");
            }
        }

        return Ok(new ApiResponse<bool>()
        {
            Data = true,
            Message = "成功上傳",
            Code = 200
        });
    }

    // https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/embedding-generation?pivots=programming-language-csharp#generating-embeddings-yourself
    private async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string text)
    {
        // Get DI Registered OpenAiTextEmbeddingGenerationService
        var textEmbeddingGenerationService = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        // Generate the embedding.
        ReadOnlyMemory<float> embedding =
            await textEmbeddingGenerationService.GenerateEmbeddingAsync(text);
        return embedding;
    }

    /// <summary>
    /// 向量搜尋飯店資訊
    /// 根據使用者輸入的查詢文字，將其向量化後進行相似度搜尋，回傳最符合的飯店清單
    /// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp
    /// </summary>
    /// <param name="query">使用者查詢文字</param>
    /// <param name="skip">略過的記錄數量(用於分頁)</param>
    /// <param name="top">回傳的最大結果數量,預設為 10</param>
    /// <returns>回傳包含飯店搜尋結果的 API 回應</returns>
    /// <exception cref="ArgumentNullException">當 Qdrant 設定遺失時拋出</exception>
    public async Task<IActionResult> VectorSearchResults([FromQuery] string query,
        [FromQuery] int skip, [FromQuery] int top = 10)
    {
        // 將使用者的查詢文字轉換為向量嵌入
        ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync(query);

        // 建立 Qdrant 用戶端
        var qdrantClient = new QdrantClient(
            host: _configuration["Qdrant:Host"] ?? throw new ArgumentNullException("Qdrant:Host"),
            apiKey: _configuration["Qdrant:ApiKey"] ?? throw new ArgumentNullException("Qdrant:ApiKey"),
            https: true
        );
        // 建立 Qdrant VectorStore 物件
        var vectorStore =
            new QdrantVectorStore(qdrantClient, ownsClient: true);
        // 從資料庫中選擇一個集合,並透過泛型參數指定儲存在其中的鍵和記錄的類型
        var collection = vectorStore.GetCollection<ulong, Hotel>("hotels");

        // 建立向量搜尋選項,並指定要搜尋的向量屬性為 DescriptionEmbedding
        var vectorSearchOptions = new VectorSearchOptions<Hotel>
        {
            VectorProperty = hotel => hotel.DescriptionEmbedding,
        };

        // 如果需要略過部分結果(分頁功能),設定 Skip 參數
        if (skip > 0)
        {
            vectorSearchOptions.Skip = skip;
        }

        // 執行向量搜尋,取得與查詢向量最相似的飯店記錄
        var searchResults = collection.SearchAsync(searchVector, top: top, vectorSearchOptions);
        var dtos = new List<HotelSearchResult>();
        // 遍歷搜尋結果,將每筆記錄轉換為 DTO 物件
        await foreach (var result in searchResults)
        {
            dtos.Add(new HotelSearchResult()
            {
                Score = result.Score ?? 0, // 相似度分數
                HotelId = result.Record.HotelId,
                Description = result.Record.Description,
                HotelName = result.Record.HotelName
            });
        }

        // 回傳搜尋結果
        return Ok(new ApiResponse<List<HotelSearchResult>>()
        {
            Data = dtos,
            Code = 200,
            Message = "Get Vector Search Results successfully.",
        });
    }

    /// <summary>
    /// 混合搜尋飯店資訊
    /// 結合向量搜尋與關鍵字搜尋，根據使用者輸入的查詢文字和關鍵字，回傳最符合的飯店清單
    /// https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/vector-search?pivots=programming-language-csharp
    /// </summary>
    /// <param name="query">使用者查詢文字，將轉換為向量進行相似度搜尋</param>
    /// <param name="keywords">關鍵字字串，以逗號分隔，用於全文檢索搜尋</param>
    /// <param name="skip">略過的記錄數量(用於分頁)</param>
    /// <param name="top">回傳的最大結果數量，預設為 10</param>
    /// <returns>回傳包含飯店搜尋結果的 API 回應</returns>
    /// <exception cref="ArgumentNullException">當 Qdrant 設定遺失時拋出</exception>
    public async Task<IActionResult> HybridSearchResults([FromQuery] string query, [FromQuery] string keywords,
        [FromQuery] int skip, [FromQuery] int top = 10)
    {
        // 將使用者的查詢文字轉換為向量嵌入
        ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync(query);

        // 建立 Qdrant 用戶端
        var qdrantClient = new QdrantClient(
            host: _configuration["Qdrant:Host"] ?? throw new ArgumentNullException("Qdrant:Host"),
            apiKey: _configuration["Qdrant:ApiKey"] ?? throw new ArgumentNullException("Qdrant:ApiKey"),
            https: true
        );
        // 建立 Qdrant VectorStore 物件
        var vectorStore =
            new QdrantVectorStore(qdrantClient, ownsClient: true);
        // 從資料庫中選擇一個集合，並透過泛型參數指定儲存在其中的鍵和記錄的類型
        var collection =  (IKeywordHybridSearchable<Hotel>)vectorStore.GetCollection<ulong, Hotel>("hotels");

        // 建立混合搜尋選項，並指定要搜尋的向量屬性為 DescriptionEmbedding
        // 以及全文檢索屬性為 Description
        var vectorSearchOptions = new HybridSearchOptions<Hotel>
        {
            VectorProperty = hotel => hotel.DescriptionEmbedding,
            AdditionalProperty = hotel => hotel.Description
        };

        // 如果需要略過部分結果(分頁功能)，設定 Skip 參數
        if (skip > 0)
        {
            vectorSearchOptions.Skip = skip;
        }

        // 將使用者輸入的關鍵字字串轉換為文字陣列
        var keywordArray = string.IsNullOrWhiteSpace(keywords) 
            ? new string[0] 
            : keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // 執行混合搜尋，結合向量相似度與關鍵字匹配，取得最相關的飯店記錄
        var searchResults = collection.HybridSearchAsync(searchVector, keywordArray, top: top, vectorSearchOptions);
        var dtos = new List<HotelSearchResult>();
        // 遍歷搜尋結果，將每筆記錄轉換為 DTO 物件
        await foreach (var result in searchResults)
        {
            dtos.Add(new HotelSearchResult()
            {
                Score = result.Score ?? 0, // 相似度分數
                HotelId = result.Record.HotelId,
                Description = result.Record.Description,
                HotelName = result.Record.HotelName
            });
        }

        // 回傳搜尋結果
        return Ok(new ApiResponse<List<HotelSearchResult>>()
        {
            Data = dtos,
            Code = 200,
            Message = "成功取得混合搜尋結果",
        });
    }
}