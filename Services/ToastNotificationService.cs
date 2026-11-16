namespace Inventory_Flow.Services;

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}

public class ToastNotificationItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public ToastType Type { get; set; } = ToastType.Success;
    public int DurationSeconds { get; set; } = 4;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public interface IToastNotificationService
{
    event Action<ToastNotificationItem>? OnShow;
    IReadOnlyList<ToastNotificationItem> GetActiveToasts();
    void Dismiss(Guid id);
    void ShowSuccess(string message, string? title = null);
    void ShowError(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowInfo(string message, string? title = null);
    void Success(string message, string? title = null);
    void Error(string message, string? title = null);
    void Warning(string message, string? title = null);
    void Info(string message, string? title = null);
}

public class ToastNotificationService : IToastNotificationService
{
    private readonly List<ToastNotificationItem> _activeToasts = new();
    private readonly object _lock = new();

    public event Action<ToastNotificationItem>? OnShow;

    public IReadOnlyList<ToastNotificationItem> GetActiveToasts()
    {
        lock (_lock)
        {
            // Limpia toasts expirados (más de 10 segundos)
            _activeToasts.RemoveAll(t => (DateTime.UtcNow - t.CreatedAt).TotalSeconds > 10);
            return _activeToasts.ToList();
        }
    }

    public void Dismiss(Guid id)
    {
        lock (_lock)
        {
            _activeToasts.RemoveAll(t => t.Id == id);
        }
    }

    private void AddAndNotify(string message, string? title, ToastType type)
    {
        var item = new ToastNotificationItem
        {
            Message = message,
            Title = title,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _activeToasts.Add(item);
        }

        OnShow?.Invoke(item);
    }

    public void ShowSuccess(string message, string? title = null) => AddAndNotify(message, title, ToastType.Success);
    public void ShowError(string message, string? title = null) => AddAndNotify(message, title, ToastType.Error);
    public void ShowWarning(string message, string? title = null) => AddAndNotify(message, title, ToastType.Warning);
    public void ShowInfo(string message, string? title = null) => AddAndNotify(message, title, ToastType.Info);

    public void Success(string message, string? title = null) => ShowSuccess(message, title);
    public void Error(string message, string? title = null) => ShowError(message, title);
    public void Warning(string message, string? title = null) => ShowWarning(message, title);
    public void Info(string message, string? title = null) => ShowInfo(message, title);
}
