using System;

namespace LegalCheck.Domain;

public sealed class EntryAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // public Guid PersonId { get; set; } // Nested in Person for now

    public DateTimeOffset AttemptedAt { get; set; }
    public string BorderPoint { get; set; } = default!; // z.B. Flughafen FRA, Grenzübergang A3
    public BorderCrossingMode Mode { get; set; }
    public bool IsAtDesignatedCrossingPoint { get; set; } // §13: zugelassene Grenzübergangsstellen
    public BorderControlStatus ControlStatus { get; set; }

    // §13 Abs.2: “eingereist” erst nach Passieren der Grenzübergangsstelle
    public bool? HasPassedBorderControlPoint { get; set; }

    // Dokumente / Voraussetzungen
    public bool? HasValidPassportOrSubstitute { get; set; } // §14 knüpft u.a. an Pass/Passersatz an
    public bool? HasRequiredResidenceTitleOrVisa { get; set; }
    public string? VisaType { get; set; }                  // z.B. Schengen C, National D
    public DateTimeOffset? VisaValidUntil { get; set; }

    // Zweck/Absicht
    public string? IntendedPurpose { get; set; }           // "Study","Work","Visit"
    public bool? IntendsEmployment { get; set; }

    // Entscheidung an der Grenze
    public EntryDecisionType Decision { get; set; }
    public string? DecisionReason { get; set; }            // human readable
    public bool? ExceptionVisaIssuedAtBorder { get; set; } // §14 Abs.2 Ausnahme-Visum

    public string? Notes { get; set; }
}
