using WebApplication1.Common;
using WebApplication1.Models;

namespace WebApplication1.Services;

public interface IProductDetailRecommendationService
{
    Task<OperationResult<CreateProductResponse>> CreateProductAsync(CreateProductRequest request);
}