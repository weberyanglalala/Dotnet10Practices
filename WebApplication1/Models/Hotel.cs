namespace WebApplication1.Models;

using Microsoft.Extensions.VectorData;

/// <summary>
/// 範例資料模型：示範如何以屬性(Attribute)定義向量存放區(Vector Store)的資料模型。
/// 依據 Microsoft Semantic Kernel 文件所述，使用下列屬性：
///  - <see cref="VectorStoreKeyAttribute"/>：定義唯一鍵
///  - <see cref="VectorStoreDataAttribute"/>：定義一般/可索引/全文索引之資料欄位
///  - <see cref="VectorStoreVectorAttribute"/>：定義向量欄位及其維度/距離函式/索引類型
/// 參考文件：https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model?pivots=programming-language-csharp
/// </summary>
public class Hotel
{
    /// <summary>
    /// [zh-TW] 記錄唯一識別鍵。向量存放區用此鍵來唯一識別每筆資料。
    /// </summary>
    [VectorStoreKey]
    public ulong HotelId { get; set; }

    /// <summary>
    /// [zh-TW] 一般資料欄位，並建立索引(IsIndexed=true)。
    /// 可用於依名稱進行篩選/等值查詢，但非全文檢索。
    /// </summary>
    [VectorStoreData(IsIndexed = true)]
    public string HotelName { get; set; }

    /// <summary>
    /// [zh-TW] 可全文索引(IsFullTextIndexed=true)之文字內容。
    /// 適合配合關鍵字/全文搜尋使用。
    /// </summary>
    [VectorStoreData(IsFullTextIndexed = true)]
    public string Description { get; set; }

    /// <summary>
    /// [zh-TW] 向量欄位：存放 <see cref="Description"/> 的向量化嵌入(Embedding)。
    /// - Dimensions：向量維度(此處為示例值 1536)
    /// - DistanceFunction：相似度度量(例如 CosineSimilarity)
    /// - IndexKind：向量索引類型(例如 Hnsw)
    /// </summary>
    [VectorStoreVector(Dimensions: 1536, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
}