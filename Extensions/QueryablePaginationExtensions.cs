using Inventory_Flow.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Flow.Extensions;

public static class QueryablePaginationExtensions
{
    public static async Task<PagedResultDto<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        var safePage = pageNumber < 1 ? 1 : pageNumber;
        var safeSize = pageSize < 1 ? 10 : (pageSize > 100 ? 100 : pageSize);

        var totalCount = await query.CountAsync();
        var totalPages = totalCount <= 0 ? 1 : (int)Math.Ceiling((double)totalCount / safeSize);

        if (safePage > totalPages)
        {
            safePage = 1;
        }

        var items = await query
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync();

        return new PagedResultDto<T>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = safePage,
            PageSize = safeSize
        };
    }
}
