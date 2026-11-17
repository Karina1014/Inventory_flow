using System;
using System.Collections.Generic;

namespace Inventory_Flow.Models;

/// <summary>
/// Códigos de estado de requisición y transiciones permitidas.
/// Evita "magic strings" en la lógica de negocio.
/// </summary>
public static class RequisitionStatusCodes
{
    public const string Solicitado = "SOLICITADO";
    public const string EnRevision = "EN_REVISION";
    public const string AprobadoParcial = "APROBADO_PARCIAL";
    public const string Despachado = "DESPACHADO";
    public const string Completado = "COMPLETADO";
    public const string Rechazado = "RECHAZADO";
    public const string Cancelado = "CANCELADO";

    /// <summary>Máquina de estados: desde qué estado se puede pasar a cuáles.</summary>
    private static readonly Dictionary<string, string[]> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
    {
        [Solicitado]      = new[] { EnRevision, Rechazado, Cancelado },
        [EnRevision]      = new[] { AprobadoParcial, Despachado, Rechazado },
        [AprobadoParcial] = new[] { Despachado, Cancelado },
        [Despachado]      = new[] { Completado },
        [Completado]      = Array.Empty<string>(),
        [Rechazado]       = Array.Empty<string>(),
        [Cancelado]       = Array.Empty<string>()
    };

    public static bool CanTransition(string fromCode, string toCode)
    {
        return AllowedTransitions.TryGetValue(fromCode, out var targets)
               && Array.Exists(targets, t => string.Equals(t, toCode, StringComparison.OrdinalIgnoreCase));
    }
}
