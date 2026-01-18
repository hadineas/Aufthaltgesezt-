using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_15_ReturnAtBorderRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_15_ReturnAtBorder";
    public string[] Citations => new[] { "AufenthG §15", "AufenthG §14" }; // Zurückweisung + unerlaubte Einreise

    public RuleResult Evaluate(CaseContext ctx)
    {
        var a = ctx.CurrentEntryAttempt;
        if (a is null)
             return RuleResult.Success(
                 "No entry attempt provided.",
                 new RuleReference("AufenthG", "§ 15", "Check")
             );

        // Wenn bereits entschieden wurde, geben wir das transparent aus:
        if (a.Decision == EntryDecisionType.Returned)
            return RuleResult.Failure(
                $"Returned at border: {a.DecisionReason ?? "reason not recorded"}",
                new RuleReference("AufenthG", "§ 15", "Returned")
            );

        if (a.Decision == EntryDecisionType.Refused)
            return RuleResult.Failure(
                $"Entry refused: {a.DecisionReason ?? "reason not recorded"}",
                new RuleReference("AufenthG", "§ 15", "Refused")
            );

        // Wenn noch keine Entscheidung: Precheck auf Basis §14-Fakten
        if (a.HasValidPassportOrSubstitute == false || a.HasRequiredResidenceTitleOrVisa == false)
        {
             // Not a failure of the rule per se, but an indicator that §15 MIGHT apply.
             // However, strictly speaking, §15 is an action by authorities. 
             // If no decision made yet, we just warn.
             return RuleResult.Failure(
                 "Attempted illegal entry indicators present; §15 return/refusal may apply (precheck only).",
                 new RuleReference("AufenthG", "§ 15", "Warning")
             );
        }

        return RuleResult.Success(
            "No return/refusal indicators based on provided facts (precheck only).",
            new RuleReference("AufenthG", "§ 15", "Check")
        );
    }
}
