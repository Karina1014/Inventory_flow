using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface IProductRepository
{
    ICollection<Product> GetProducts();
    Product? GetProduct(int IdProduct);
    bool IsUniqueName(string name);
    Task<Product> CreateProduct(CreateProductDto createProductDto);
    Task<Product> UpdateProduct(int IdProduct, UpdateProductDto updateProductDto);
    bool DeleteProduct(int IdProduct);
}
