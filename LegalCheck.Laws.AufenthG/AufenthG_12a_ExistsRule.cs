using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_12a_ExistsRule : IFactualRule
{
    public string RuleId => "FACT_AufenthG_12a_Exists";
    public string[] Citations => new[] { "AufenthG §12a" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var o = ctx.ResidenceObligation12a;

        if (o is null || !o.IsActive)
        {
            return RuleResult.Success(
                "No active residence obligation under §12a.",
                new RuleReference("AufenthG", "§ 12a", "Residence Obligation")
            );
        }

        if (o.ValidUntil is not null && o.ValidUntil <= ctx.AsOfDate)
        {
             return RuleResult.Success(
                "Residence obligation existed but is no longer valid.",
                new RuleReference("AufenthG", "§ 12a", "Residence Obligation")
            );
        }

        return RuleResult.Failure(
             $"Residence obligation applies to area {o.FederalStateCode}" +
                (o.MunicipalityCode is not null ? $" / {o.MunicipalityCode}" : ""),
             new RuleReference("AufenthG", "§ 12a", "Residence Obligation")
        );
    }
}
