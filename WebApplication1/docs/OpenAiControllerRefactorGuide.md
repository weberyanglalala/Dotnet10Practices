# OpenAiController 重構詳細說明

## 重構概述

本次重構將 `OpenAiController.CreateProduct` 方法中的業務邏輯提取到專門的服務層 `OpenAiService`，並引入 `OperationResult<T>` 作為統一的服務回應格式，以提升程式碼的可維護性、可測試性和一致性。

## 重構前後對比

### 重構前架構

```
OpenAiController
├── CreateProduct 方法
    ├── 直接建立 ChatClient
    ├── 讀取配置 (OpenAiApiKey)
    ├── 建立 ChatMessage 列表
    ├── 設定 ChatCompletionOptions (包含 JSON Schema)
    ├── 呼叫 OpenAI API
    ├── 解析 JSON 回應
    ├── 手動建立 CreateProductResponse
    └── 直接回傳 ApiResponse
```

### 重構後架構

```
OpenAiController
├── CreateProduct 方法
    ├── 呼叫 OpenAiService.CreateProductAsync
    ├── 檢查 OperationResult.IsSuccess
    ├── 成功時回傳 ApiResponse
    └── 失敗時使用 Problem() 方法

OpenAiService
├── CreateProductAsync 方法
    ├── 驗證配置
    ├── 建立 ChatClient
    ├── 準備 ChatMessages
    ├── 設定 JSON Schema
    ├── 呼叫 OpenAI API
    ├── 錯誤處理
    ├── 解析與驗證回應
    └── 回傳 OperationResult<CreateProductResponse>
```

## 詳細變更內容

### 1. 使用現有 OperationResult<T> 類別

**檔案位置**: `Common/OperationResult.cs` (已存在)

**用途**: 提供統一的服務層回應格式，包含成功/失敗狀態、資料、錯誤訊息和狀態碼。

**使用方式**:
- 成功時: `OperationResult<T>.Success(data)`
- 失敗時: `OperationResult<T>.Failure(errorMessage, statusCode)`

### 2. 建立 OpenAiService 服務

**檔案位置**: `Services/OpenAiService.cs`

**目的**: 將 OpenAI API 互動邏輯從控制器提取到服務層。

**主要改進**:

- **依賴注入**: 使用 `IConfiguration` 注入配置
- **配置驗證**: 檢查 OpenAI API Key 是否存在
- **錯誤處理**: 完善的異常處理和記錄
  - API 通訊錯誤
  - JSON 解析錯誤
  - 配置遺失錯誤
  - 資料驗證錯誤
- **回應驗證**: 檢查 API 回應內容的完整性
- **統一回應**: 使用 `OperationResult<T>` 作為回傳型別
- **關注點分離**: 將 OpenAI 特定邏輯與控制器分離

**具體實作**:

```csharp
public async Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
{
    // 1. 驗證配置 (OpenAI API Key)
    // 2. 建立 ChatClient
    // 3. 準備系統訊息和使用者訊息
    // 4. 設定 JSON Schema 格式
    // 5. 呼叫 OpenAI API
    // 6. 處理各種異常情況
    // 7. 解析 JSON 回應
    // 8. 驗證必要欄位
    // 9. 建立 CreateProductResponse 物件
    // 10. 回傳 OperationResult
}
```

**JSON Schema 管理**:

將 JSON Schema 提取為私有方法或常數，提升可讀性：

```csharp
private static BinaryData GetTravelItinerarySchema()
{
    return BinaryData.FromBytes("""
    {
      "type": "object",
      "properties": {
        "brand": { "type": "string", "description": "旅遊品牌名稱" },
        "name": { "type": "string", "description": "形如 '品牌名稱｜景點/描述五日遊'" },
        // ... 其他屬性
      },
      "required": ["brand", "name", "category", ...],
      "additionalProperties": false
    }
    """u8.ToArray());
}
```

### 3. 建立 IOpenAiService 介面

**檔案位置**: `Services/IOpenAiService.cs`

**目的**: 定義服務契約，支援依賴注入和單元測試。

**介面定義**:

```csharp
public interface IOpenAiService
{
    Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request);
}
```

**優點**:
- 支援介面導向程式設計
- 方便建立模擬物件進行單元測試
- 未來可替換不同的實作（例如：Azure OpenAI Service）

### 4. 重構 OpenAiController

**檔案位置**: `Controllers/OpenAi/OpenAiController.cs`

**主要變更**:

- **建構函式**: 注入 `IOpenAiService` 而非 `IConfiguration`
- **方法簡化**: `CreateProduct` 方法大幅簡化，專注於 HTTP 層面的處理
- **回應處理**: 根據 `OperationResult.IsSuccess` 決定回應方式
- **錯誤處理**: 使用 `Problem()` 方法處理失敗情況，提供標準化的錯誤回應

**重構後的 CreateProduct 方法**:

```csharp
[HttpPost]
public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
{
    var result = await _openAiService.CreateProductAsync(request);

    if (result.IsSuccess)
    {
        return Ok(new ApiResponse<CreateProductResponse>
        {
            Data = result.Data,
            Code = 200,
            Message = "成功"
        });
    }

    return Problem(
        detail: result.ErrorMessage,
        statusCode: result.Code
    );
}
```

### 5. 服務註冊

**檔案位置**: `Program.cs`

**變更內容**:

```csharp
// 新增服務註冊
builder.Services.AddScoped<IOpenAiService, OpenAiService>();
```

**服務生命週期選擇**:
- 使用 `Scoped` 生命週期，每個 HTTP 請求建立一個實例
- 適合包含狀態或使用其他 scoped 服務的情境
- 如果服務完全無狀態，也可考慮使用 `Singleton`

## 設計模式應用

### 1. 服務層模式 (Service Layer Pattern)

將業務邏輯從控制器提取到專門的服務類別，控制器僅負責：
- 接收 HTTP 請求
- 呼叫服務方法
- 將服務結果轉換為 HTTP 回應

### 2. 結果模式 (Result Pattern)

使用 `OperationResult<T>` 統一服務方法的回傳格式，優點：
- 明確表達操作成功或失敗
- 避免使用異常來處理業務邏輯錯誤
- 提供結構化的錯誤資訊

### 3. 依賴注入 (Dependency Injection)

使用建構函式注入服務依賴：
- 提高程式碼的鬆耦合性
- 支援單元測試（可注入模擬物件）
- 符合 SOLID 原則中的依賴反轉原則

### 4. 介面隔離 (Interface Segregation)

定義 `IOpenAiService` 介面：
- 依賴抽象而非具體實作
- 支援未來替換不同的 AI 服務提供者
- 提升程式碼的可測試性

## 錯誤處理策略

### 服務層錯誤處理

```csharp
public async Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
{
    try
    {
        // 1. 配置驗證
        var apiKey = _configuration["OpenAiApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("OpenAI API Key 未設定");
            return OperationResult<CreateProductResponse>.Failure(
                "OpenAI API Key 未設定",
                500
            );
        }

        // 2. 呼叫 OpenAI API
        ChatClient client = new("gpt-4.1", apiKey);
        // ... 建立訊息和選項
        ChatCompletion completion = await client.CompleteChatAsync(messages, options);

        // 3. 解析回應
        using JsonDocument json = JsonDocument.Parse(completion.Content[0].Text);
        // ... 建立回應物件

        return OperationResult<CreateProductResponse>.Success(response);
    }
    catch (Exception ex)
    {
        // 捕捉所有錯誤（包括 API 錯誤、JSON 解析錯誤等）
        _logger.LogError(ex, "建立產品時發生未預期的錯誤");
        return OperationResult<CreateProductResponse>.Failure(
            "內部伺服器錯誤",
            500
        );
    }
}
```

### 控制器層錯誤處理

```csharp
var result = await _openAiService.CreateProductAsync(request);

if (result.IsSuccess)
{
    return Ok(new ApiResponse<CreateProductResponse>
    {
        Data = result.Data,
        Code = 200,
        Message = "成功"
    });
}

return Problem(
    detail: result.ErrorMessage,
    statusCode: result.Code
);
```

**錯誤回應範例**:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Gateway",
  "status": 502,
  "detail": "OpenAI API 錯誤: Invalid API Key"
}
```

## 安全性改進

### 配置驗證

- 檢查 OpenAI API Key 是否存在
- 避免因配置錯誤導致執行時期錯誤
- 提早發現配置問題

### 錯誤訊息控制

- 不暴露內部 API Key 或敏感資訊給客戶端
- 記錄詳細錯誤資訊到日誌供除錯使用
- 回傳適當的 HTTP 狀態碼和一般性錯誤訊息

### API Key 管理

**建議做法**:

1. **開發環境**: 使用 User Secrets
   ```bash
   dotnet user-secrets set "OpenAiApiKey" "your-api-key"
   ```

2. **生產環境**: 使用環境變數或 Azure Key Vault
   ```bash
   export OpenAiApiKey="your-api-key"
   ```

3. **永不將 API Key 寫死在程式碼中**

## 擴展性考量

### 新增 OpenAI 功能

現在可以輕鬆在 `OpenAiService` 中新增更多方法：

```csharp
public interface IOpenAiService
{
    Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request);
    Task<OperationResult<string>> GenerateDescriptionAsync(string title);
    Task<OperationResult<List<string>>> GenerateKeywordsAsync(string content);
}
```

### 支援多個 AI 提供者

可以建立抽象介面，支援不同的 AI 服務：

```csharp
public interface IAiProductGenerator
{
    Task<OperationResult<CreateProductResponse>> GenerateProductAsync(CreateProductRequest request);
}

public class OpenAiProductGenerator : IAiProductGenerator { }
public class AzureOpenAiProductGenerator : IAiProductGenerator { }
public class ClaudeProductGenerator : IAiProductGenerator { }

// 在 Program.cs 選擇實作
builder.Services.AddScoped<IAiProductGenerator, OpenAiProductGenerator>();
```

### JSON Schema 版本管理

將 JSON Schema 提取為獨立檔案或常數類別：

```csharp
public static class TravelItinerarySchemas
{
    public static BinaryData V1 => BinaryData.FromBytes("""..."""u8.ToArray());
    public static BinaryData V2 => BinaryData.FromBytes("""..."""u8.ToArray());
}
```

## 遷移指南

### 步驟 1: 建立服務介面

建立 `Services/IOpenAiService.cs`，定義服務契約。

### 步驟 2: 實作服務類別

建立 `Services/OpenAiService.cs`，將控制器中的邏輯搬移至服務。

### 步驟 3: 註冊服務

在 `Program.cs` 中註冊 `IOpenAiService` 和 `OpenAiService`。

### 步驟 4: 更新控制器

修改 `OpenAiController`：
- 注入 `IOpenAiService`
- 簡化 `CreateProduct` 方法
- 使用 `OperationResult` 處理回應

### 步驟 5: 測試

- 撰寫單元測試驗證服務邏輯
- 撰寫整合測試驗證端到端流程
- 手動測試各種成功和失敗情境

### 步驟 6: 更新文件

更新 API 文件說明新的錯誤回應格式。

## 向後相容性

### API 回應格式

成功回應格式保持不變：

```json
{
  "data": {
    "isSuccess": true,
    "brand": "探索台灣",
    "name": "探索台灣｜陽明山溫泉三日遊",
    ...
  },
  "code": 200,
  "message": "成功"
}
```

錯誤回應改為標準 Problem Details 格式（RFC 7807）：

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Gateway",
  "status": 502,
  "detail": "OpenAI API 錯誤: Rate limit exceeded"
}
```

**注意**: 客戶端需要更新錯誤處理邏輯以支援新的錯誤格式。

## 最佳實踐總結

### 控制器職責

- 僅處理 HTTP 層面的事務
- 驗證請求（可使用 FluentValidation）
- 呼叫服務方法
- 將服務結果轉換為 HTTP 回應

### 服務層職責

- 實作業務邏輯
- 與外部服務互動（OpenAI API）
- 錯誤處理和記錄
- 回傳結構化的結果物件

### 錯誤處理

- 在服務層捕捉並處理異常
- 使用 `OperationResult` 表達失敗狀態
- 記錄詳細錯誤資訊供除錯
- 回傳適當的 HTTP 狀態碼

## 檢查清單

重構完成後，請確認以下項目：

- [x] `IOpenAiService` 介面已建立
- [x] `OpenAiService` 實作已完成
- [x] 服務已在 `Program.cs` 註冊
- [x] `OpenAiController` 已簡化並使用服務
- [x] 錯誤處理已完善實作
- [x] 配置驗證已實作
- [x] 日誌記錄已加入關鍵位置
- [ ] API 文件已更新

## 總結

本次重構成功將原本緊耦合的控制器邏輯分離到專門的服務層，引入統一的結果處理模式，並大幅提升了程式碼的可維護性、可測試性和擴展性。遵循了 SOLID 原則和現代 ASP.NET Core 最佳實踐，為未來支援更多 AI 功能或替換 AI 提供者奠定了良好的基礎。

## 參考資源

- [OperationResult Pattern](https://enterprisecraftsmanship.com/posts/error-handling-exception-or-result/)
- [Service Layer Pattern](https://martinfowler.com/eaaCatalog/serviceLayer.html)
- [ASP.NET Core Dependency Injection](https://learn.microsoft.com/aspnet/core/fundamentals/dependency-injection)
- [OpenAI .NET SDK Documentation](https://github.com/openai/openai-dotnet)
- [Problem Details for HTTP APIs (RFC 7807)](https://tools.ietf.org/html/rfc7807)
