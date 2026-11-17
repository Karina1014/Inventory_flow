using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models.Dtos;

public class InternalRequisitionDto
{
    public int IdInternalRequisition { get; set; }
    public string RequisitionCode { get; set; } = string.Empty;
    public DateTime RequisitionDate { get; set; }

    public int IdRequestingWarehouse { get; set; }
    public string RequestingWarehouseName { get; set; } = string.Empty;
    public int IdFulfillingWarehouse { get; set; }
    public string FulfillingWarehouseName { get; set; } = string.Empty;

    public int IdUser { get; set; }
    public string UserName { get; set; } = string.Empty;

    // Tipo (prioridad / SLA)
    public int IdRequisitionType { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int TypePriorityLevel { get; set; }
    public int TypeMaxResponseHours { get; set; }

    // Estado (flujo)
    public int IdRequisitionStatus { get; set; }
    public string StatusCode { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusBadgeColor { get; set; } = string.Empty;
    public bool StatusIsFinal { get; set; }

    public string Observation { get; set; } = string.Empty;
    public DateTime? ReviewedDate { get; set; }
    public int? IdReviewedByUser { get; set; }
    public string? ReviewedByUserName { get; set; }
    public string? ReviewNote { get; set; }

    public int TotalRequestedQuantity { get; set; }
    public int TotalDispatchedQuantity { get; set; }

    public List<InternalRequisitionDetailDto> Details { get; set; } = new();
}
