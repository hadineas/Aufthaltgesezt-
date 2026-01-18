using System;

namespace LegalCheck.Domain;

/// <summary>
/// A snapshot of facts for rule evaluation. 
/// Decouples the Rule Engine from the persistent Person entity.
/// </summary>
public record CaseContext(
    Guid PersonId,
    DateTimeOffset AsOfDate,

    // Core
    string NationalityIso2,
    bool IsEuCitizen, 
    
    // Status
    string? CurrentResidenceTitleCode, // derived from active permit
    int TotalResidenceMonths, // calculated from history
    bool HasGermanDegree, // calculated from education

    // Raw History for granular rules (e.g. § 9c)
    IEnumerable<ResidencePermit> ResidencePermits,
    IEnumerable<ResidencePeriod> ResidencePeriods,
    
    // Asylum Context (§ 10)
    AsylumProfile? AsylumProfile,
    
    // § 12a
    ResidenceObligation12a? ResidenceObligation12a,
    string? CurrentResidenceAreaCode,

    // § 13-15 Entry
    EntryAttempt? CurrentEntryAttempt,
    
    // § 15a
    DistributionProcedure15a? Distribution15a,
    
    // § 16-16f
    IReadOnlyList<EducationPurposeCase> EducationCases,

    // Evidence
    IReadOnlyList<EvidenceDocument> Documents,

    // Integration
    LanguageLevel LanguageLevel,
    bool IntegrationCourseCompleted,
    bool CommitsToFdgo,

    // Economics
    bool IsLivelihoodSecured, // Manual overrides or deep calc
    decimal MonthlyNetIncome, // Sum of current employments
    decimal MonthlyHousingCost,

    // Conduct
    // Conduct
    bool HasCriminalRecord,
    
    // Legal Constraints (§ 11)
    bool HasEntryBan,
    DateTimeOffset? EntryBanUntil,

    // Missing Data / Flags
    bool HasValidPassport
);
