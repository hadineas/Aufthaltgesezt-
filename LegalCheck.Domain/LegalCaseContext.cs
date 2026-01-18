namespace LegalCheck.Domain;

public class LegalCaseContext
{
    public Applicant Person { get; }
    
    // Future: Add other context like "CurrentDate", "ProcessingAuthority", "FamilyMembers"
    
    public LegalCaseContext(Applicant person)
    {
        Person = person;
    }
}
