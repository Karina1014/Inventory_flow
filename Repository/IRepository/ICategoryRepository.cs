using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface ICategoryRepository
{
    ICollection<Category> GetCategories();
    Category? GetCategory(int IdCategory);
    bool IsUniqueName(string name);
    Task<Category> CreateCategory(CreateCategoryDto createCategoryDto);
    Task<Category> UpdateCategory(int IdCategory, UpdateCategoryDto updateCategoryDto);
    bool DeleteCategory(int IdCategory);
}
