using System;
using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 18e - Short-term Mobility for Researchers
public sealed class AufenthG_18e_MobilityRule : IFactualRule
{
    public string RuleId => "Fact_18e_ResearchMobility";
    public string[] Citations => new[] { "AufenthG §18e" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.ResearchMobility_18e);
        if (c is null) return RuleResult.Success("No §18e mobility case.", new RuleReference("AufenthG", "§ 18e", "Skip"));

        // 1. Valid Title from other Member State (Abstracted check)
        // In a real system, we'd check 'ctx.OtherEuTitles' or similar. 
        // For now, let's assume if they apply for this case, they claim to have one.
        // We warn if we can't verify it, but let's proceed to specific fields.
        
        // 2. Hosting Agreement
        if (c.HasHostingAgreement != true)
            return RuleResult.Failure("Missing Hosting Agreement (§ 18e Abs. 1 Nr. 2).", 
                new RuleReference("AufenthG", "§ 18e Abs. 1 Nr. 2", "Agreement Missing"));

        // 3. BAMF Notification
        if (c.BamfNotificationDate == null)
            return RuleResult.Failure("BAMF has not been notified (§ 18e Abs. 1).", 
                new RuleReference("AufenthG", "§ 18e Abs. 1", "Notification Missing"));

        // 4. Livelihood
        if (ctx.IsLivelihoodSecured != true)
             return RuleResult.Failure("Livelihood not secured (§ 18e Abs. 1 Nr. 4).", 
                 new RuleReference("AufenthG", "§ 18e Abs. 1 Nr. 4", "Livelihood"));

        // 5. Duration max 180 days in 360 days
        if (c.PlannedStayDays > 180)
             return RuleResult.Failure($"Planned stay {c.PlannedStayDays} days exceeds limit of 180 days for short-term mobility.", 
                 new RuleReference("AufenthG", "§ 18e Abs. 1", "Duration Exceeded"));

        // Success means "Exempt from Residence Title requirement"
        return RuleResult.Success("Requirements for Short-term Research Mobility met (Exempt from Title).", 
            new RuleReference("AufenthG", "§ 18e Abs. 2", "Exempt"));
    }
}
