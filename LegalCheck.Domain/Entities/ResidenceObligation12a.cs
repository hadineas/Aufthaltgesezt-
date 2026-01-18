using System;

namespace LegalCheck.Domain;

public sealed class ResidenceObligation12a
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    // Verpflichteter Wohnort
    public string FederalStateCode { get; set; } = default!; // z.B. "DE-NW"
    public string? MunicipalityCode { get; set; }            // optional: Stadt/Kreis

    // Rechtsgrund
    public string LegalBasis { get; set; } = "§12a AufenthG";

    // Zeitraum
    public DateTimeOffset ImposedAt { get; set; }
    public DateTimeOffset? ValidUntil { get; set; }

    // Ausnahmen
    public bool EmploymentException { get; set; }        // Arbeit außerhalb erlaubt?
    public bool EducationException { get; set; }         // Ausbildung/Studium?
    public bool FamilyException { get; set; }            // Familiennachzug?
    public string? ExceptionNotes { get; set; }

    public bool IsActive { get; set; } = true;
}
