using System;

namespace LegalCheck.Domain;

public sealed class PermitCondition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // In a real DB, you'd link to PermitId, but for our in-memory domain record it's nested.
    // public Guid ResidencePermitId { get; set; }

    public PermitConditionType Type { get; set; }
    public string? Value { get; set; }               // z.B. "nur Bayern", "Landkreis X", "Beschäftigung nur mit Genehmigung"
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }

    // Traceability
    public string LegalBasis { get; set; } = "AufenthG §12";
    public string? Notes { get; set; }
    
    public PermitCondition(PermitConditionType type, string? value = null)
    {
        Type = type;
        Value = value;
    }
}
