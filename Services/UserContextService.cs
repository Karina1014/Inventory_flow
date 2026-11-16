using System.Security.Claims;

namespace Inventory_Flow.Services;

public interface IUserContextService
{
    int UserId { get; }
    string Username { get; }
    string Role { get; }
    bool IsAdmin { get; }
    int? WarehouseId { get; }
    bool IsAuthenticated { get; }
}

public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int UserId
    {
        get
        {
            var claimVal = Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? Principal?.FindFirst("idUser")?.Value;
            return int.TryParse(claimVal, out var id) ? id : 0;
        }
    }

    public string Username => Principal?.FindFirst(ClaimTypes.Name)?.Value 
                           ?? Principal?.FindFirst("username")?.Value 
                           ?? string.Empty;

    public string Role => Principal?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    public int? WarehouseId
    {
        get
        {
            var claimVal = Principal?.FindFirst("warehouse_id")?.Value;
            return int.TryParse(claimVal, out var wId) ? wId : null;
        }
    }
}

