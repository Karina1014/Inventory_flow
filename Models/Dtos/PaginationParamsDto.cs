namespace Inventory_Flow.Models.Dtos;

public class PaginationParamsDto
{
    public static readonly int[] AllowedPageSizes = new[] { 10, 20, 50, 100 };

    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    private int _pageNumber = 1;
    private int _pageSize = DefaultPageSize;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? DefaultPageSize : value);
    }

    public string? Search { get; set; }
}
