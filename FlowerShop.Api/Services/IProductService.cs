using FlowerShop.Api.Models.DTOs;

namespace FlowerShop.Api.Services;

public interface IProductService
{
    Task<List<CategoryResponse>> GetCategoriesAsync();
    Task<PagedResult<ProductListResponse>> GetProductsAsync(ProductListRequest request);
    Task<ProductDetailResponse?> GetProductByIdAsync(long id);
    Task<List<SkuResponse>> GetProductSkusAsync(long productId);
}
