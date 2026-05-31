using FlowerShop.Api.Models;
using FlowerShop.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly IProductService _productService;

    public CategoriesController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _productService.GetCategoriesAsync();
        return Ok(ApiResponse<List<Models.DTOs.CategoryResponse>>.Success(categories));
    }
}
