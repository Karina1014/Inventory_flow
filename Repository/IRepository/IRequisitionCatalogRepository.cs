using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;

namespace Inventory_Flow.Repository.IRepository;

/// <summary>Acceso de solo lectura a los catálogos de estados y tipos de requisición.</summary>
public interface IRequisitionCatalogRepository
{
    Task<ICollection<RequisitionStatus>> GetStatusesAsync();
    Task<ICollection<RequisitionType>> GetTypesAsync(bool activeOnly = true);
    Task<RequisitionStatus?> GetStatusByCodeAsync(string code);
    Task<RequisitionType?> GetTypeByIdAsync(int idRequisitionType);
}
