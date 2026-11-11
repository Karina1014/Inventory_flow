using Inventory_Flow.Data;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;
using Inventory_Flow.Repository.IRepository;

namespace Inventory_Flow.Repository;

public class CategoryRepository : ICategoryRepository
{
    private readonly AplicationDbContext _db;

    public CategoryRepository(AplicationDbContext db)
    {
        _db = db;
    }

    public ICollection<Category> GetCategories()
    {
        return _db.Categories.OrderBy(c => c.Name).ToList();
    }

    public Category? GetCategory(int IdCategory)
    {
        return _db.Categories.FirstOrDefault(c => c.IdCategory == IdCategory);
    }

    public bool IsUniqueName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return false;
        return !_db.Categories.Any(c => c.Name != null && c.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<Category> CreateCategory(CreateCategoryDto createCategoryDto)
    {
        var category = new Category()
        {
            Name = createCategoryDto.Name,
            Description = createCategoryDto.Description,
            CreationDate = DateTime.Now
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateCategory(int IdCategory, UpdateCategoryDto updateCategoryDto)
    {
        var category = await _db.Categories.FindAsync(IdCategory)
            ?? throw new KeyNotFoundException($"Categoría con Id {IdCategory} no encontrada.");

        category.Name = updateCategoryDto.Name;
        category.Description = updateCategoryDto.Description;

        _db.Categories.Update(category);
        await _db.SaveChangesAsync();
        return category;
    }

    public bool DeleteCategory(int IdCategory)
    {
        var category = _db.Categories.FirstOrDefault(c => c.IdCategory == IdCategory);
        if (category == null) return false;

        _db.Categories.Remove(category);
        _db.SaveChanges();
        return true;
    }
}
