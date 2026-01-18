using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_9c_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_9c";
    public string[] Citations => new[] { "AufenthG §9c" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var reasons = new List<string>();

        // Check permits
        if (ctx.ResidencePermits != null)
        {
            foreach (var p in ctx.ResidencePermits)
            {
                if (p.ExcludedFromLongTermEU == true)
                    reasons.Add($"Permit {p.PermitTypeCode} marked as excluded from Daueraufenthalt-EU.");

                if (p.IsAsylumProcedureOnly == true)
                    reasons.Add($"Permit {p.PermitTypeCode} is asylum-procedure-only.");

                if (p.IsTemporaryPurpose == true)
                    reasons.Add($"Permit {p.PermitTypeCode} is temporary purpose.");
                
                if (p.IsHumanitarianPermit == true) // Added based on requirement logic
                    reasons.Add($"Permit {p.PermitTypeCode} is humanitarian permit (potentially excluded).");
            }
        }

        // Check periods
        if (ctx.ResidencePeriods != null)
        {
            foreach (var rp in ctx.ResidencePeriods)
            {
                if (rp.IsExcludedBy9c == true)
                    reasons.Add($"Residence period (Start: {rp.StartDate}) excluded by §9c: {rp.ExclusionReason9c}");
                
                if (rp.IsTemporaryStay == true)
                    reasons.Add($"Residence period (Start: {rp.StartDate}) marked as temporary stay.");
            }
        }

        if (reasons.Count == 0)
        {
            return RuleResult.Success(
                "No §9c exclusion indicators found.", 
                new RuleReference("AufenthG", "§ 9c", "Precheck")
            );
        }

        return RuleResult.Failure(
            "§ 9c exclusion indicators found.", 
            new RuleReference("AufenthG", "§ 9c", "Precheck")
        );
    }
}
