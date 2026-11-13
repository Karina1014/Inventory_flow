using System;

namespace Inventory_Flow.Services;

public class CurrentUserService
{
    public string Name { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsAuthenticated { get; private set; } = false;

    public event Action? OnChange;

    public void SetUser(string name, string username, string role)
    {
        Name = string.IsNullOrWhiteSpace(name) ? username : name;
        Username = username;
        Role = string.IsNullOrWhiteSpace(role) ? string.Empty : role;
        IsAuthenticated = true;
        NotifyStateChanged();
    }

    public void Clear()
    {
        Name = string.Empty;
        Username = string.Empty;
        Role = string.Empty;
        IsAuthenticated = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}

