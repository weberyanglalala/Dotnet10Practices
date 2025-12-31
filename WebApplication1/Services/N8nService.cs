using System.Net.Http.Headers;
using System.Text.Json;
using WebApplication1.Common;
using WebApplication1.Models;

namespace WebApplication1.Services;

public class N8nService : IProductDetailRecommendationService
{
    private readonly ILogger<N8nService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public N8nService(ILogger<N8nService> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var endpoint = _configuration["N8nWebhookEndpoint"];

            if (string.IsNullOrEmpty(endpoint))
            {
                _logger.LogError("N8nWebhookEndpoint configuration is missing");
                return OperationResult<CreateProductResponse>.Failure("N8n webhook endpoint not configured", 500);
            }

            var apiKey = _configuration["N8nApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("N8nApiKey configuration is missing");
                return OperationResult<CreateProductResponse>.Failure("N8n API key not configured", 500);
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            _logger.LogInformation("Sending request to N8n webhook: {Endpoint}", endpoint);

            var response = await client.PostAsJsonAsync(endpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("N8n webhook returned error status {StatusCode}: {Error}", response.StatusCode, errorContent);
                return OperationResult<CreateProductResponse>.Failure($"N8n webhook error: {response.StatusCode}", (int)response.StatusCode);
            }

            var responseString = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseString))
            {
                _logger.LogError("N8n webhook returned empty response");
                return OperationResult<CreateProductResponse>.Failure("Empty response from N8n webhook", 502);
            }

            var result = JsonSerializer.Deserialize<CreateProductResponse>(responseString);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize N8n response: {Response}", responseString);
                return OperationResult<CreateProductResponse>.Failure("Invalid response format from N8n webhook", 502);
            }

            _logger.LogInformation("Successfully created product via N8n webhook");
            return OperationResult<CreateProductResponse>.Success(result, 200);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request error when calling N8n webhook");
            return OperationResult<CreateProductResponse>.Failure("Failed to connect to N8n webhook", 502);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error when processing N8n response");
            return OperationResult<CreateProductResponse>.Failure("Invalid JSON response from N8n webhook", 502);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling N8n webhook");
            return OperationResult<CreateProductResponse>.Failure("Internal server error", 500);
        }
    }
}