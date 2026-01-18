using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_12a_ComplianceRule : IFactualRule
{
    public string RuleId => "COMPLIANCE_AufenthG_12a";
    public string[] Citations => new[] { "AufenthG §12a" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var o = ctx.ResidenceObligation12a;

        if (o is null || !o.IsActive)
             return RuleResult.Success(
                 "No active §12a obligation.",
                 new RuleReference("AufenthG", "§ 12a", "Compliance")
             );


        if (ctx.CurrentResidenceAreaCode is null)
             return RuleResult.Failure(
                 "Current residence location unknown.",
                 new RuleReference("AufenthG", "§ 12a", "Compliance")
             );

        if (ctx.CurrentResidenceAreaCode.StartsWith(o.FederalStateCode))
        {
             return RuleResult.Success(
                 "Current residence complies with §12a obligation.",
                 new RuleReference("AufenthG", "§ 12a", "Compliance")
             );
        }

         return RuleResult.Failure(
            $"Current residence ({ctx.CurrentResidenceAreaCode}) outside obligated area {o.FederalStateCode}.",
            new RuleReference("AufenthG", "§ 12a", "Compliance")
        );
    }
}
