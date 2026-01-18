using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_15a_DistributionStatusRule : IFactualRule
{
    public string RuleId => "FACT_AufenthG_15a_DistributionStatus";
    public string[] Citations => new[] { "AufenthG §15a" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var d = ctx.Distribution15a;
        if (d is null)
            return RuleResult.Success(
                "No §15a distribution procedure recorded.",
                new RuleReference("AufenthG", "§ 15a", "Status")
            );

        return RuleResult.Failure(
            $"§15a status: {d.Status}. Assigned state: {d.AssignedFederalStateCode ?? "unknown"}.",
            new RuleReference("AufenthG", "§ 15a", "Procedure Active")
        );
    }
}
