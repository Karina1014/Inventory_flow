using System.Collections.Generic;
using System.Threading.Tasks;
using Inventory_Flow.Models;
using Inventory_Flow.Models.Dtos;

namespace Inventory_Flow.Repository.IRepository;

public interface ISupplierRepository
{
    ICollection<Supplier> GetSuppliers();
    Task<PagedResultDto<Supplier>> GetSuppliersPagedAsync(int pageNumber, int pageSize, string? search = null, bool? activeOnly = null);
    Supplier? GetSupplier(int idSupplier);
    bool IsUniqueRuc(string ruc, int? excludeId = null);
    Task<Supplier> CreateSupplier(CreateSupplierDto createSupplierDto);
    Task<Supplier> UpdateSupplier(int idSupplier, UpdateSupplierDto updateSupplierDto);
    bool DeleteSupplier(int idSupplier);
}
