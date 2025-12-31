using Microsoft.AspNetCore.Mvc;
using WebApplication1.Common;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers.OpenAi;

[ApiController]
[Route("api/[controller]/[action]")]
public class OpenAiController : ControllerBase
{
    private readonly IProductDetailRecommendationService _openAiService;

    public OpenAiController(IProductDetailRecommendationService openAiService)
    {
        _openAiService = openAiService;
    }

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
}