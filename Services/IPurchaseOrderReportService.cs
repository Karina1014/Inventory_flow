using System.Threading.Tasks;

namespace Inventory_Flow.Services;

public interface IPurchaseOrderReportService
{
    /// <summary>
    /// Genera en memoria el archivo PDF oficial de una orden de compra usando la plantilla RDLC de Report Builder.
    /// </summary>
    /// <param name="idPurchaseOrder">Identificador de la orden de compra a generar.</param>
    /// <returns>Arreglo de bytes del archivo PDF generado.</returns>
    Task<byte[]> GeneratePurchaseOrderPdfAsync(int idPurchaseOrder);
}
