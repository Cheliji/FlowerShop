using FlowerShop.Api.Models;
using FlowerShop.Api.Models.DTOs;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductListRequest request)
    {
        var result = await _productService.GetProductsAsync(request);
        return Ok(ApiResponse<PagedResult<ProductListResponse>>.Success(result));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(long id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return Ok(ApiResponse<ProductDetailResponse>.Fail(404, "商品不存在"));
        }
        return Ok(ApiResponse<ProductDetailResponse>.Success(product));
    }

    [HttpGet("{id}/skus")]
    public async Task<IActionResult> GetProductSkus(long id)
    {
        var skus = await _productService.GetProductSkusAsync(id);
        return Ok(ApiResponse<List<SkuResponse>>.Success(skus));
    }
}
