using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AplicationDbContext _db;

    public ProductRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Product> GetProducts()
    {
        return _db.Products
            .OrderByDescending(p => p.CreationDate)
            .ThenByDescending(p => p.IdProduct)
            .ToList();
    }

    public async Task<PagedResultDto<Product>> GetProductsPagedAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        int? categoryId = null,
        string statusFilter = "all",
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _db.Products.Include(p => p.Category).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                (p.Name != null && p.Name.Contains(term)) ||
                (p.Code != null && p.Code.Contains(term)) ||
                (p.Description != null && p.Description.Contains(term)));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(p => p.IdCategory == categoryId.Value);
        }

        if (statusFilter == "active")
        {
            query = query.Where(p => p.IsActive);
        }
        else if (statusFilter == "inactive")
        {
            query = query.Where(p => !p.IsActive);
        }

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(p => p.CreationDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(p => p.CreationDate < end);
        }

        return await query
            .OrderByDescending(p => p.CreationDate)
            .ThenByDescending(p => p.IdProduct)
            .ToPagedResultAsync(pageNumber, pageSize);
    }

    public Product? GetProduct(int IdProduct)
    {
        return _db.Products.FirstOrDefault(p => p.IdProduct == IdProduct);
    }

    public bool IsUniqueName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return !_db.Products.Any(p => p.Name != null && p.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Product> CreateProduct(CreateProductDto createProductDto)
    {
        var product = new Product()
        {
            Code = createProductDto.Code,
            Name = createProductDto.Name,
            Description = createProductDto.Description,
            SuggestedPrice = createProductDto.SuggestedPrice,
            Iva = createProductDto.Iva,
            IsActive = createProductDto.IsActive,
            IdCategory = createProductDto.IdCategory,
            CreationDate = DateTime.Now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateProduct(int IdProduct, UpdateProductDto updateProductDto)
    {
        var product = await _db.Products.FindAsync(IdProduct)
            ?? throw new KeyNotFoundException($"Producto con Id {IdProduct} no encontrado.");

        product.Code = updateProductDto.Code;
        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
        product.SuggestedPrice = updateProductDto.SuggestedPrice;
        product.Iva = updateProductDto.Iva;
        product.IsActive = updateProductDto.IsActive;
        product.IdCategory = updateProductDto.IdCategory;

        _db.Products.Update(product);
        await _db.SaveChangesAsync();
        return product;
    }

    public bool DeleteProduct(int IdProduct)
    {
        var product = _db.Products.FirstOrDefault(p => p.IdProduct == IdProduct);
        if (product == null) return false;

        _db.Products.Remove(product);
        _db.SaveChanges();
        return true;
    }
}
