using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;

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
        return _db.Products.OrderBy(p => p.Name).ToList();
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
            Name = createProductDto.Name,
            Description = createProductDto.Description,
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

        product.Name = updateProductDto.Name;
        product.Description = updateProductDto.Description;
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
