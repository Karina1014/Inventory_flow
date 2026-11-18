namespace Inventory_Flow.Models.Dtos.Reports;

public class PurchaseOrderReportHeaderDto
{
    // Datos de la Empresa (MedVet Enterprise)
    public string CompanyName { get; set; } = "MEDVET ENTERPRISE - CLÍNICA Y HOSPITAL VETERINARIO";
    public string CompanyRuc { get; set; } = "1792345678001";
    public string CompanyAddress { get; set; } = "Av. Galo Plaza Lasso y Panamericana Norte, Matriz Carapungo, Quito - Ecuador";
    public string CompanyPhone { get; set; } = "(02) 289-4560 / 099-876-5432";
    public string CompanyEmail { get; set; } = "adquisiciones@medvet-enterprise.ec";

    // Datos de la Orden de Compra
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public string ExpectedDeliveryDate { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string DestinationWarehouseName { get; set; } = string.Empty;
    public string Observation { get; set; } = string.Empty;

    // Datos del Proveedor
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierRuc { get; set; } = string.Empty;
    public string SupplierContact { get; set; } = string.Empty;
    public string SupplierPhone { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddress { get; set; } = string.Empty;

    // Totales Monetarios
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Trazabilidad y Firmas
    public string CreatedByUserName { get; set; } = string.Empty;
    public string FinancialApprovalUserName { get; set; } = string.Empty;
    public string FinancialApprovalDate { get; set; } = string.Empty;
    public string FinancialApprovalNote { get; set; } = string.Empty;
}
