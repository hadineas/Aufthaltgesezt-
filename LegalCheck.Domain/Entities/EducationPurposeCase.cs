using System;

namespace LegalCheck.Domain;

public sealed class EducationPurposeCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public bool? HasUniversityEntranceQualification { get; set; } // Hochschulzugangsberechtigung

    // § 18e Research Mobility
    public bool? HasHostingAgreement { get; set; } // Aufnahmevereinbarung
    public DateTimeOffset? BamfNotificationDate { get; set; } // Mitteilung an BAMF

    // Common
    public EducationPurposeType PurposeType { get; set; }

    public AdmissionType AdmissionType { get; set; }
    public bool? HasAdmissionOrContract { get; set; }  // z.B. Uni-Zulassung / Ausbildungsvertrag etc.
    public DateTimeOffset? PlannedStart { get; set; }
    public DateTimeOffset? PlannedEnd { get; set; }

    // §16c Mobility-specific (<= 360 Tage)
    public int? PlannedStayDays { get; set; }
    public bool? IsShortTermMobility { get; set; } // “§16c-Szenario” Indikator

    // Arbeitsmarkt/BA-Fakten (v.a. §16a, §16e)
    public bool? RequiresBAApproval { get; set; }
    public bool? HasBAApproval { get; set; }

    // Erlaubte Nebenbeschäftigung
    public int? AllowedWorkHoursPerWeek { get; set; } 

    // Lebensunterhalt / Versicherung
    public bool? HasSecuredLivelihood { get; set; }
    public string? HealthInsuranceType { get; set; } // "Public"|"Private"|"Unknown"

    public string? Notes { get; set; }
}
