using System;

namespace Inventory_Flow.Services;

public enum ExpirationStatus
{
    Valid,
    ExpiringSoon,
    Expired
}

public static class LotExpiration
{
    public const int DefaultAlertDays = 30;

    public static int GetAlertDays(IConfiguration? configuration)
    {
        var days = configuration?.GetValue<int?>("InventorySettings:ExpirationAlertDays");
        return days is > 0 ? days.Value : DefaultAlertDays;
    }

    public static int DaysToExpire(DateTime expirationDate)
        => (expirationDate.Date - DateTime.Today).Days;

    public static ExpirationStatus GetStatus(DateTime expirationDate, int alertDays)
    {
        var days = DaysToExpire(expirationDate);
        if (days < 0) return ExpirationStatus.Expired;
        if (days <= alertDays) return ExpirationStatus.ExpiringSoon;
        return ExpirationStatus.Valid;
    }
}
