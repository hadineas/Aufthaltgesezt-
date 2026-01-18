using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_12_ConditionsPrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_12_Conditions";
    public string[] Citations => new[] { "AufenthG §12" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var now = ctx.AsOfDate;

        // Flatten all conditions from all permits available in the context
        var activeConditions = ctx.ResidencePermits
            .SelectMany(p => p.Conditions.Select(c => new { Permit = p, Condition = c }))
            .Where(x =>
                (x.Condition.ValidFrom is null || x.Condition.ValidFrom <= now) &&
                (x.Condition.ValidUntil is null || x.Condition.ValidUntil > now))
            .ToList();

        if (activeConditions.Count == 0)
        {
            return RuleResult.Success(
                "No active permit conditions (Nebenbestimmungen) recorded.",
                new RuleReference("AufenthG", "§ 12", "Conditions")
            );
        }

        var reasons = activeConditions.Select(x =>
            $"Active condition on {x.Permit.PermitTypeCode}: {x.Condition.Type} {(string.IsNullOrWhiteSpace(x.Condition.Value) ? "" : $"- {x.Condition.Value}")}".Trim()
        ).ToArray();

        // NotSatisfied here means "Restrictions exist", which is a warning/info state for the UI,
        // or a blocker for specific entitlements depending on the downstream logic.
        return RuleResult.Failure(
            "Active permit conditions (restrictions) found.",
            new RuleReference("AufenthG", "§ 12", "Conditions")
        );
    }
}
