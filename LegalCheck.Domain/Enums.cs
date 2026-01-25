namespace LegalCheck.Domain;

public enum LanguageLevel
{
    A1 = 1,
    A2 = 2,
    B1 = 3, // Minimum for standard naturalization
    B2 = 4,
    C1 = 5, // Triggers "Turbo" clause (3 years)
    C2 = 6
}

public enum ResidenceTitleType
{
    // Permanent
    Niederlassungserlaubnis, // Settlement Permit
    ErlaubnisZumDaueraufenthaltEU, // Long-term EU

    // Temporary but eligible for path
    BlueCardEU,
    Fachkraft, // Skilled Worker (§ 18a, 18b)
    HumanitarianProtection, // § 25 Abs. 1, 2

    // Generally Excluded (Require switch)
    Student, // § 16b
    JobSeeker, // § 20
    Tourist,
    Duldung // Tolerated stay
}

public enum AsylumStatus
{
    None = 0,              // Nie Asyl beantragt
    Pending = 1,           // Asylverfahren läuft
    Recognized = 2,        // Schutzstatus anerkannt
    Rejected = 3,          // Abgelehnt
    RejectedManifest = 4,  // Offensichtlich unbegründet
    Withdrawn = 5
}

public enum PermitConditionType
{
    Unknown = 0,
    SpatialRestriction = 1,    // räumliche Beschränkung (§12 Abs.2/3)
    ResidenceObligation = 2,   // Wohnsitzauflage (systematisch oft §12a)
    EmploymentRestriction = 3, // Beschäftigungsauflage/-verbot
    ReportingDuty = 4,         // Meldeauflage
    Other = 99
}

public enum BorderCrossingMode { Unknown=0, Air=1, Land=2, Sea=3 }
public enum EntryDecisionType { Unknown=0, Allowed=1, Refused=2, Returned=3 }
public enum BorderControlStatus { Unknown=0, NotChecked=1, Checked=2 }

public enum EducationPurposeType
{
    Unknown = 0,
    VocationalTraining_16a = 1,
    Study_16b = 2,
    StudyMobility_16c = 3,
    RecognitionMeasure_16d = 4,
    EUInternship_16e = 5,
    LanguageOrSchool_16f = 6,
    
    // § 17
    SearchForTraining_17_1 = 7,
    StudyApplication_17_2 = 8,
    
    // § 18e
    ResearchMobility_18e = 9,
    
    // § 18f
    ResearchMobility_18f = 10
}

public enum AdmissionType
{
    Unknown = 0,
    UniversityAdmission,           // Zulassung Hochschule (§16b)
    TrainingContract,              // Ausbildungsvertrag (§16a)
    RecognitionPlan,               // Anerkennungs-/Qualifizierungsplan (§16d)
    InternshipAgreementEU,          // Praktikumsvereinbarung (§16e)
    LanguageCourseEnrollment,       // Sprachkurs (§16f)
    SchoolEnrollmentOrExchange      // Schulbesuch/Schüleraustausch (§16f)
}

public enum DistributionStatus15a
{
    NotApplicable = 0,
    InProcedure = 1,
    Assigned = 2,
    RelocationRequested = 3,
    RelocationGranted = 4,
    Completed = 5
}

public enum EvidenceType
{
    Unknown = 0,
    MatriculationCertificate = 1,
    UniversityAdmission = 2,
    TrainingContract = 3,
    Passport = 4,
    Visa = 5,
    ProfessionalLicense = 6,
    EquivalenceDecision = 7,  // Bescheid über Gleichwertigkeit
    Other = 99
}

public enum QualificationType
{
    None = 0,
    VocationalTraining = 1, // Berufsausbildung
    AcademicDegree = 2      // Hochschulabschluss
}

public enum EquivalenceStatus
{
    NotChecked = 0,
    Confirmed = 1,          // Anerkannt / Gleichwertig festgestellt
    PartiallyRecognized = 2,
    NotEquivalant = 3
}

public enum EmployeeMobilityType
{
    None = 0,
    BlueCardBusiness_18h = 1,
    BlueCardLongTerm_18i = 2
}

public enum ICTRole
{
    None = 0,
    Manager = 1,
    Specialist = 2,
    Trainee = 3
}
