using System;

namespace LegalCheck.Domain;

/// <summary>
/// Represents facts related to Employment for § 18 (Skilled Workers).
/// </summary>
public sealed class EmploymentCase
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // The Job Offer
    public bool HasConcreteJobOffer { get; set; }
    public string JobTitle { get; set; } = default!;
    public string IscoCode { get; set; } = string.Empty; // For Blue Card § 18g
    public int JobDurationMonths { get; set; } // Min 6 months for § 18g
    
    public decimal MonthlySalaryGross { get; set; }
    
    // Approvals
    public bool HasBAApproval { get; set; } // Bundesagentur für Arbeit
    public bool RequiresProfessionalLicense { get; set; } // Berufsausübungserlaubnis needed?
    public bool HasProfessionalLicense { get; set; } // Has it?

    // Qualifications
    public QualificationType Qualification { get; set; }
    public EquivalenceStatus Equivalence { get; set; }
    public bool IsGermanDegree { get; set; } // If true, Equivalence implicit
    public DateOnly? GraduationDate { get; set; } // For "Fresh Grad" Blue Card

    public bool HasITSpecialistExperience { get; set; } // § 18g Abs. 2 (No degree IT)
    
    // § 18h
    public EmployeeMobilityType MobilityType { get; set; }
    // Using JobDurationMonths for long term, but for mobility we need Days.
    // Let's reuse 'JobDurationMonths' * 30 approximation or add specific field.
    // Adding field for clarity.
    public int PlannedMobilityDays { get; set; }

    // § 19 ICT Card
    public bool IsICT { get; set; }
    public ICTRole IctRole { get; set; }
    public int PriorGroupEmploymentMonths { get; set; } // > 6 months
    public bool HasReturnGuarantee { get; set; } // § 19 Abs. 2 Nr. 4b

    // For > 45 years old (§ 18 Abs. 2 Nr. 5)
    public bool HasAdequatePensionPlan { get; set; } // verification separate
}
