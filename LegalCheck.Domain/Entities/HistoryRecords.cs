using System;

namespace LegalCheck.Domain;

public enum AbsenceReason
{
    Unknown = 0,
    AssignmentByEmployer = 1,
    StudyOrTraining = 2,
    FamilyReasons = 3,
    Other = 99
}

public sealed record ResidencePeriod(
    DateOnly StartDate,
    DateOnly? EndDate,
    string CountryIso2, // Default "DE"
    string Basis, // e.g., "Visa", "Permit", "EU-FreeMovement"
    // § 9b Fields
    bool? IsLawful = null,
    bool? HadGermanTitleDuringPeriod = null,
    AbsenceReason AbsenceReason = AbsenceReason.Unknown,
    bool? CountsForLongTermEu = null,
    // §9c-relevant
    bool? IsTemporaryStay = null,          // nur vorübergehend?
    bool? IsExcludedBy9c = null,           // Ergebnis eines Prechecks
    string? ExclusionReason9c = null       // rein erklärend
);

public record ResidencePermit(
    string PermitTypeCode, // e.g., "§ 16b"
    DateOnly IssuedAt,
    DateOnly ValidUntil,
    DateOnly ValidUntil,
    string Purpose, // "Study", "Work"
    // NEW for §9c
    bool? IsTemporaryPurpose = null,        // nur vorübergehend?
    bool? IsHumanitarianPermit = null,      // §25 ff?
    bool? IsAsylumProcedureOnly = null,     // Asylverfahren?
    bool? ExcludedFromLongTermEU = null     // expliziter Ausschluss
)
{
    public ICollection<PermitCondition> Conditions { get; set; } = new List<PermitCondition>();
}

public record EducationRecord(
    string Level, // "Bachelor", "Master", "Vocational"
    bool DidGraduate,
    DateOnly? GraduationDate,
    string FieldOfStudy,
    string Institution
);

public record EmploymentRecord(
    string EmploymentType, // "Employee", "SelfEmployed"
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal MonthlyNetIncome
);
