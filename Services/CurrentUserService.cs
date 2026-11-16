using System;

namespace Inventory_Flow.Services;

public class CurrentUserService
{
    public int UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string Role { get; private set; } = string.Empty;
    public int? WarehouseId { get; private set; }
    public string? WarehouseName { get; private set; }
    public bool IsAuthenticated { get; private set; } = false;

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    public event Action? OnChange;

    public void SetUser(int userId, string name, string username, string role, int? warehouseId = null, string? warehouseName = null, string? email = null, string? phone = null)
    {
        UserId = userId;
        Name = string.IsNullOrWhiteSpace(name) ? username : name;
        Username = username;
        Role = string.IsNullOrWhiteSpace(role) ? string.Empty : role;
        WarehouseId = warehouseId;
        WarehouseName = warehouseName;
        Email = email;
        Phone = phone;
        IsAuthenticated = true;
        NotifyStateChanged();
    }

    public void UpdateProfileData(string? name, string? email, string? phone)
    {
        Name = string.IsNullOrWhiteSpace(name) ? Username : name;
        Email = email;
        Phone = phone;
        NotifyStateChanged();
    }

    public void SetUser(string name, string username, string role)
    {
        SetUser(0, name, username, role, null, null, null, null);
    }

    public void Clear()
    {
        UserId = 0;
        Name = string.Empty;
        Username = string.Empty;
        Email = null;
        Phone = null;
        Role = string.Empty;
        WarehouseId = null;
        WarehouseName = null;
        IsAuthenticated = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

