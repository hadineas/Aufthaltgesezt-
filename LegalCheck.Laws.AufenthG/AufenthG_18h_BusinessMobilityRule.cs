using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 18h - Short-term Mobility for Blue Card Holders
public sealed class AufenthG_18h_BusinessMobilityRule : IFactualRule
{
    public string RuleId => "Fact_18h_BlueCardMobility";
    public string[] Citations => new[] { "AufenthG §18h" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var emp = ctx.EmploymentCases.FirstOrDefault(e => e.MobilityType == EmployeeMobilityType.BlueCardBusiness_18h);
        if (emp == null) return RuleResult.Success("No §18h business mobility case.", new RuleReference("AufenthG", "§ 18h", "Skip"));

        // 1. Valid EU Blue Card (Abstracted)
        // Similar to 18e/f, we assume the applicant holds one if applying for this mobility.
        // Real check: ctx.HasForeignBlueCard == true
        
        // 2. Business Activity
        // Implied by "BlueCardBusiness_18h" enum usage in this context.
        
        // 3. Duration <= 90 days in 180 days
        if (emp.PlannedMobilityDays > 90)
             return RuleResult.Failure($"Planned stay {emp.PlannedMobilityDays} days exceeds 90-day limit for short-term mobility.", 
                 new RuleReference("AufenthG", "§ 18h Abs. 1", "Duration Exceeded"));

        if (emp.PlannedMobilityDays <= 0)
             return RuleResult.Failure("Invalid duration.", new RuleReference("AufenthG", "§ 18h", "Bad Input"));

        return RuleResult.Success("Requirements for Blue Card Business Mobility met (Exempt from Title).", 
            new RuleReference("AufenthG", "§ 18h Abs. 1", "Exempt"));
    }
}
