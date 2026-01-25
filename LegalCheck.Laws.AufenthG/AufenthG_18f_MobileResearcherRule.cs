using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 18f - Residence Permit for Mobile Researchers (Long-term)
public sealed class AufenthG_18f_MobileResearcherRule : IFactualRule
{
    public string RuleId => "Fact_18f_MobileResearcher";
    public string[] Citations => new[] { "AufenthG §18f" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.ResearchMobility_18f);
        if (c is null) return RuleResult.Success("No §18f mobility case.", new RuleReference("AufenthG", "§ 18f", "Skip"));

        // 1. Valid Title from other Member State (Abstracted)
        // Assume verified if application is for this purpose in Demo.
        
        // 2. Passport
        if (!ctx.HasValidPassport)
             return RuleResult.Failure("Valid passport required (§ 18f Abs. 1 Nr. 2).", 
                 new RuleReference("AufenthG", "§ 18f Abs. 1 Nr. 2", "Passport Missing"));

        // 3. Hosting Agreement
        if (c.HasHostingAgreement != true)
            return RuleResult.Failure("Missing Hosting Agreement (§ 18f Abs. 1 Nr. 3).", 
                new RuleReference("AufenthG", "§ 18f Abs. 1 Nr. 3", "Agreement Missing"));

        // 4. Duration > 180 days AND <= 360 days (1 Year)
        // § 18f Abs. 1: "mehr als 180 Tage und höchstens ein Jahr"
        if (c.PlannedStayDays <= 180)
             return RuleResult.Failure($"Planned stay {c.PlannedStayDays} days too short for § 18f (must be > 180). Use § 18e.", 
                 new RuleReference("AufenthG", "§ 18f Abs. 1", "Too Short"));
                 
        if (c.PlannedStayDays > 365) // Using 365 for "ein Jahr"
             return RuleResult.Failure($"Planned stay {c.PlannedStayDays} days exceeds 1 year limit.", 
                 new RuleReference("AufenthG", "§ 18f Abs. 1", "Too Long"));

        return RuleResult.Success("Entitlement to residence permit for mobile researcher exists.", 
            new RuleReference("AufenthG", "§ 18f Abs. 1", "Grant"));
    }
}
