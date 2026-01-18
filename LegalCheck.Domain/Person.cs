using System;

namespace LegalCheck.Domain;

public class Person
{
    public Guid PersonId { get; private set; }
    public Guid OwnerUserId { get; private set; } // The user who manages this profile
    
    // Core Identity
    public string Name { get; set; } = string.Empty; // Optional/Pseudonym
    public string NationalityIso2 { get; set; } = "XX"; // e.g. "DE", "TR", "SY"
    public DateOnly? BirthDate { get; set; }
    
    // Asylum Context
    public AsylumProfile? AsylumProfile { get; set; }
    
    // § 12a Residence Obligation
    public ResidenceObligation12a? ResidenceObligation12a { get; set; }
    
    // § 13-15 Border Entry
    public EntryAttempt? LastEntryAttempt { get; set; }
    
    // § 15a Distribution
    public DistributionProcedure15a? Distribution15a { get; set; }

    // § 16-16f Education
    public List<EducationPurposeCase> EducationCases { get; set; } = new();

    // Documents
    public List<EvidenceDocument> Documents { get; set; } = new();

    // Immigration Status
    public List<ResidencePermit> Permits { get; set; } = new();
    public List<ResidencePeriod> ResidenceHistory { get; set; } = new();
    public List<EducationRecord> EducationHistory { get; set; } = new();
    public List<EmploymentRecord> EmploymentHistory { get; set; } = new();

    // Integration / Skills
    public LanguageLevel GermanLanguageLevel { get; set; }
    public bool IntegrationCourseCompleted { get; set; }
    public bool CommitsToFreeDemocraticOrder { get; set; } = true;
    public bool AcceptsGermanBasicLaw { get; set; } = true;

    // Economic Situation (Now calculated or manual override)
    public bool IsLivelihoodSecured { get; set; }
    public decimal MonthlyHousingCost { get; set; }
    
    // Conduct
    public bool HasCriminalRecord { get; set; }

    // Legal Constraints (§ 11)
    public bool HasEntryBan { get; set; } // Einreise- und Aufenthaltsverbot
    public DateOnly? EntryBanUntil { get; set; } // Befristung

    public Person(Guid ownerUserId, string name)
    {
        PersonId = Guid.NewGuid();
        OwnerUserId = ownerUserId;
        Name = name;
    }

    // Calculation helper
    public int CalculateResidenceYears(DateOnly asOfDate)
    {
        // Simple aggregator: Sum of all "StartDate -> EndDate (or Now)" where Country == DE
        // This is a naive implementation; real law requires checking gaps > 6 months etc.
        int totalDays = 0;
        foreach (var p in ResidenceHistory)
        {
            if (p.CountryIso2 != "DE") continue;
            
            var end = p.EndDate ?? asOfDate;
            if (end > asOfDate) end = asOfDate;
            if (p.StartDate > end) continue;

            totalDays += end.DayNumber - p.StartDate.DayNumber;
        }
        return totalDays / 365;
    }
}
