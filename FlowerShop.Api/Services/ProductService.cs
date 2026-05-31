using FlowerShop.Api.Data;
using FlowerShop.Api.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Api.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CategoryResponse>> GetCategoriesAsync()
    {
        return await _db.Categories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon
            })
            .ToListAsync();
    }

    public async Task<PagedResult<ProductListResponse>> GetProductsAsync(ProductListRequest request)
    {
        var query = _db.Flowers
            .Include(f => f.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(f => f.Name.Contains(keyword)
                || (f.FlowerLanguage != null && f.FlowerLanguage.Contains(keyword))
                || (f.Subtitle != null && f.Subtitle.Contains(keyword)));
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(f => f.Price >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(f => f.Price <= request.MaxPrice.Value);
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(f => f.SoldCount)
            .ThenByDescending(f => f.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(f => new ProductListResponse
            {
                Id = f.Id,
                Name = f.Name,
                Subtitle = f.Subtitle,
                FlowerLanguage = f.FlowerLanguage,
                Price = f.Price,
                OriginalPrice = f.OriginalPrice,
                MainImage = f.MainImage,
                SoldCount = f.SoldCount,
                CategoryId = f.CategoryId,
                CategoryName = f.Category.Name
            })
            .ToListAsync();

        return new PagedResult<ProductListResponse>
        {
            Items = items,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductDetailResponse?> GetProductByIdAsync(long id)
    {
        var flower = await _db.Flowers
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flower == null) return null;

        return new ProductDetailResponse
        {
            Id = flower.Id,
            Name = flower.Name,
            Subtitle = flower.Subtitle,
            FlowerLanguage = flower.FlowerLanguage,
            Description = flower.Description,
            SuitableFor = flower.SuitableFor,
            DeliveryDesc = flower.DeliveryDesc,
            Price = flower.Price,
            OriginalPrice = flower.OriginalPrice,
            Stock = flower.Stock,
            SoldCount = flower.SoldCount,
            MainImage = flower.MainImage,
            Images = flower.Images.Select(i => i.Url).ToList(),
            PriceOptions = flower.PriceOptions.Select(p => new FlowerPriceOptionDto
            {
                Quantity = p.Quantity,
                Label = p.Label,
                Price = p.Price
            }).ToList(),
            CategoryId = flower.CategoryId,
            CategoryName = flower.Category.Name
        };
    }

    public async Task<List<SkuResponse>> GetProductSkusAsync(long productId)
    {
        var flower = await _db.Flowers
            .FirstOrDefaultAsync(f => f.Id == productId);

        if (flower == null) return new List<SkuResponse>();

        return flower.PriceOptions.Select((p, index) => new SkuResponse
        {
            Id = index + 1,
            SpecName = p.Label,
            Price = p.Price,
            Stock = flower.Stock
        }).ToList();
    }
}
