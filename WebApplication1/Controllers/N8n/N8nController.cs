using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Common;
using WebApplication1.Models;
using WebApplication1.Services;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace WebApplication1.Controllers.N8n;

[ApiController]
[Route("api/[controller]/[action]")]
public class N8NController : ControllerBase
{
    private readonly IProductDetailRecommendationService _n8nService;
    private readonly IValidator<CreateProductRequest> _createProductRequestValidator;

    public N8NController(IProductDetailRecommendationService n8nService, IValidator<CreateProductRequest> createProductRequestValidator)
    {
        _n8nService = n8nService;
        _createProductRequestValidator = createProductRequestValidator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        ValidationResult validationResult = _createProductRequestValidator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key.ToLowerInvariant(),
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return BadRequest(errors);
        }
        
        var result = await _n8nService.CreateProductAsync(request);

        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<CreateProductResponse>
            {
                Data = result.Data,
                Code = result.Code,
                Message = "取得商品資料成功"
            });
        }
        else
        {
            return Problem(
                detail: result.ErrorMessage,
                statusCode: result.Code,
                title: "Create Product Failed"
            );
        }
    }
}
