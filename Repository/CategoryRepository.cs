using Inventory_Flow.Data;
using Inventory_Flow.Extensions;
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

    public async Task<PagedResultDto<Category>> GetCategoriesPagedAsync(int pageNumber, int pageSize, string? search = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.Categories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                (c.Name != null && c.Name.Contains(term)) ||
                (c.Description != null && c.Description.Contains(term)));
        }

        if (startDate.HasValue)
        {
            var start = startDate.Value.Date;
            query = query.Where(c => c.CreationDate >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(c => c.CreationDate < end);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToPagedResultAsync(pageNumber, pageSize);
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
