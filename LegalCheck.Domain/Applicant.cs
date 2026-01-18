namespace LegalCheck.Domain;

public class Applicant
{
    public string Name { get; set; } = string.Empty;
    public ResidenceTitle CurrentTitle { get; set; }
    public int YearsOfResidence { get; set; }
    public LanguageLevel GermanLanguageLevel { get; set; }
    public bool IsLivelihoodSecured { get; set; }
    public bool HasCriminalRecord { get; set; }
    public bool CommitsToFreeDemocraticOrder { get; set; } = true; // "Bekenntnis zur FDGO"
    
    // Naturalization specifics
    public decimal MonthlyNetIncome { get; set; }
    public decimal MonthlyHousingCost { get; set; }
    public bool AcceptsGermanBasicLaw { get; set; } = true; // "Einordnung in die deutschen Lebensverhältnisse"
    public DateOnly? EntryDate { get; set; } // To calculate precise residence time

    public Applicant(string name, ResidenceTitle currentTitle, int yearsOfResidence, LanguageLevel languageLevel)
    {
        Name = name;
        CurrentTitle = currentTitle;
        YearsOfResidence = yearsOfResidence;
        GermanLanguageLevel = languageLevel;
    }
}
