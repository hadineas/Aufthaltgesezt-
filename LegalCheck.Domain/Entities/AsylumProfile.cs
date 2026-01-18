using System;

namespace LegalCheck.Domain;

public sealed class AsylumProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public AsylumStatus Status { get; set; }

    public DateTimeOffset? ApplicationDate { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }

    public bool? IsDeportable { get; set; }          // vollziehbar ausreisepflichtig?
    public bool? HasDepartureBan { get; set; }       // Abschiebungsverbot?
    public bool? HasTemporarySuspension { get; set; } // Duldung (§60a)

    // §10-relevant
    public bool? EntryWithVisa { get; set; }         // mit Visum eingereist?
    public bool? IdentityClarified { get; set; }     // Identität geklärt?
}
