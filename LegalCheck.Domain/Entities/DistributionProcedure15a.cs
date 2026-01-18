using System;

namespace LegalCheck.Domain;

public sealed class DistributionProcedure15a
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public DateTimeOffset DetectedAt { get; set; } // Feststellung unerlaubte Einreise
    public DistributionStatus15a Status { get; set; }

    public string? AssignedFederalStateCode { get; set; } // z.B. "DE-NW"
    public bool? HasNoAsylumRequest { get; set; }          // Abgrenzung zu Asyl (Fakt)
    public bool? InDetentionAndRemovable { get; set; }     // wenn unmittelbar abschiebbar etc. (Fakt)
    public bool? CertificateIssued { get; set; }           // Verfahrensbescheinigung

    public string? Notes { get; set; }
}
