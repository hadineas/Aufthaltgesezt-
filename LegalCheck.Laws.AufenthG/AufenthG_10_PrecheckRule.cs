using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_10_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_10";
    public string[] Citations => new[] { "AufenthG §10" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var asylum = ctx.AsylumProfile;
        
        // No asylum history -> Allowed
        if (asylum is null || asylum.Status == AsylumStatus.None)
        {
            return RuleResult.Success(
                "No asylum procedure recorded.",
                new RuleReference("AufenthG", "§ 10", "Precheck")
            );
        }

        // Pending -> Blocked
        if (asylum.Status == AsylumStatus.Pending)
        {
             return RuleResult.Failure(
                "Asylum procedure is still pending.",
                new RuleReference("AufenthG", "§ 10", "Precheck")
            );
        }

        // Rejected/Manifestly Rejected + Deportable -> Blocked
        if (asylum.Status is AsylumStatus.Rejected or AsylumStatus.RejectedManifest)
        {
            if (asylum.IsDeportable == true)
            {
                return RuleResult.Failure(
                    "Asylum rejected and person is deportable.",
                    new RuleReference("AufenthG", "§ 10", "Precheck")
                );
            }
        }

        // Recognized or specific exceptions -> Warning / Pass but noted
        // For now, satisfy with note.
        return RuleResult.Success(
            "Asylum check passed (Recognized or non-deportable).",
            new RuleReference("AufenthG", "§ 10", "Precheck")
        );
    }
}
