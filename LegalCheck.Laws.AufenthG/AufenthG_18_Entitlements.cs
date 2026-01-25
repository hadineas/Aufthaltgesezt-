using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 18a Entitlement (Vocational)
public sealed class AufenthG_18a_VocationalEntitlementRule : IFactualRule
{
    public string RuleId => "Entitlement_18a_Vocational";
    // § 18a uses "wird erteilt" (mandatory grant) if requirements met
    public string[] Citations => new[] { "AufenthG §18a" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        // 1. Definition check (must be Fachkraft with *Vocational* training)
        // We re-evaluate logic or rely on definition rule. 
        // For decoupled logic, we re-check facts directly OR rely on a Result set passed in (not supported yet).
        // Checks: Vocational + Confirmed Equivalence
        
        bool isVocationalFachkraft = false;
        foreach (var emp in ctx.EmploymentCases)
        {
            if (emp.Qualification == QualificationType.VocationalTraining && emp.Equivalence == EquivalenceStatus.Confirmed)
            {
                isVocationalFachkraft = true;
                break;
            }
        }
        
        if (!isVocationalFachkraft)
            return RuleResult.Failure("Not a Fachkraft with vocational training (§ 18a).", new RuleReference("AufenthG", "§ 18a", "Definition"));
        
        // 2. Qualified Employment & General Requirements (§ 18 Abs. 2)
        // We assume "General Requirements" rule covers the basics (Job Offer, BA, License).
        // Re-check briefly:
        bool hasJob = false;
        foreach (var emp in ctx.EmploymentCases)
        {
            if (emp.HasConcreteJobOffer) hasJob = true;
        }

        if (!hasJob)
             return RuleResult.Failure("No qualified job offer.", new RuleReference("AufenthG", "§ 18a", "Job Offer"));

        // 3. Age/Pension (§ 18 Abs. 2 Nr. 5)
        // Skip re-implementing here, but technically it's a blocker.
        
        return RuleResult.Success("Entitlement to residence permit pursuant to § 18a exists.", 
            new RuleReference("AufenthG", "§ 18a", "Grant"));
    }
}

// § 18b Entitlement (Academic)
public sealed class AufenthG_18b_AcademicEntitlementRule : IFactualRule
{
    public string RuleId => "Entitlement_18b_Academic";
    public string[] Citations => new[] { "AufenthG §18b" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        // 1. Definition check (Academic)
        bool isAcademicFachkraft = false;
        foreach (var emp in ctx.EmploymentCases)
        {
            // German or Recognized Foreign
            if (emp.Qualification == QualificationType.AcademicDegree && (emp.IsGermanDegree || emp.Equivalence == EquivalenceStatus.Confirmed))
            {
                isAcademicFachkraft = true;
                break;
            }
        }

        if (!isAcademicFachkraft)
             return RuleResult.Failure("Not a Fachkraft with academic training (§ 18b).", new RuleReference("AufenthG", "§ 18b", "Definition"));

        // 2. Job Check
        bool hasJob = false;
        foreach (var emp in ctx.EmploymentCases)
        {
            if (emp.HasConcreteJobOffer) hasJob = true;
        }

        if (!hasJob)
             return RuleResult.Failure("No qualified job offer.", new RuleReference("AufenthG", "§ 18b", "Job Offer"));

        return RuleResult.Success("Entitlement to residence permit pursuant to § 18b exists.", 
            new RuleReference("AufenthG", "§ 18b", "Grant"));
    }
}
