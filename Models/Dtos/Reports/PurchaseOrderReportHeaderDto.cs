namespace Inventory_Flow.Models.Dtos.Reports;

/// <summary>
/// DTO que suministra los datos consultados de la base de datos al DataSet del reporte RDLC.
/// Cero datos quemados: todos los campos se alimentan de SQL Server.
/// </summary>
public class PurchaseOrderReportHeaderDto
{
    // Datos de la Bodega Matriz / Empresa (Consultados de tabla Warehouses)
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyRuc { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;

    // Datos de la Orden de Compra (Consultados de tabla PurchaseOrders)
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public string Observation { get; set; } = string.Empty;

    // Datos del Proveedor (Consultados de tabla Suppliers)
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierRuc { get; set; } = string.Empty;
    public string SupplierContact { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddress { get; set; } = string.Empty;

    // Totales Monetarios (Consultados de tabla PurchaseOrders)
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Trazabilidad y Firmas (Consultados de tabla Users y PurchaseOrders)
    public string CreatedByUserName { get; set; } = string.Empty;
    public string FinancialApprovalUserName { get; set; } = string.Empty;
    public string FinancialApprovalDate { get; set; } = string.Empty;
    public string FinancialApprovalNote { get; set; } = string.Empty;
}
