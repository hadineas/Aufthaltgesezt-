using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_13_BorderCrossingStatusRule : IFactualRule
{
    public string RuleId => "FACT_AufenthG_13_BorderCrossingStatus";
    public string[] Citations => new[] { "AufenthG §13" }; // Grenzübertritt

    public RuleResult Evaluate(CaseContext ctx)
    {
        var a = ctx.CurrentEntryAttempt;
        if (a is null) 
             return RuleResult.Failure(
                 "No entry attempt provided.",
                 new RuleReference("AufenthG", "§ 13", "Insufficient Data")
             );
        // Note: RuleResult.Satisfied/NotSatisfied is binary. 
        // For pure factual rules, we use Satisfied to mean "Fact Checked".

        var reasons = new List<string>
        {
            $"Designated crossing point: {a.IsAtDesignatedCrossingPoint}.",
            $"Border control passed: {a.HasPassedBorderControlPoint?.ToString() ?? "unknown"}."
        };

        // Kein “erfüllt/nicht erfüllt” im rechtlichen Sinne – nur Faktenstatus.
        return RuleResult.Success(
            string.Join("; ", reasons),
            new RuleReference("AufenthG", "§ 13", "Status")
        );
    }
}
