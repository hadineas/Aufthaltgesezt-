using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_14_IllegalEntryPrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_14_IllegalEntry";
    public string[] Citations => new[] { "AufenthG §14" }; // Unerlaubte Einreise; Ausnahme-Visum

    public RuleResult Evaluate(CaseContext ctx)
    {
        var a = ctx.CurrentEntryAttempt;
        if (a is null) 
             return RuleResult.Success(
                 "No entry attempt provided for illegal entry check.", 
                 new RuleReference("AufenthG", "§ 14", "Precheck")
             );

        // Falls Ausnahme-Visum an der Grenze ausgestellt wurde
        if (a.ExceptionVisaIssuedAtBorder == true)
        {
            return RuleResult.Success(
                "Exception visa/pass substitute issued at border (recorded).",
                new RuleReference("AufenthG", "§ 14", "Exception Visa")
            );
        }

        // Minimaler formaler Check (nur Fakten): Pass + erforderlicher Titel/Visum
        if (a.HasValidPassportOrSubstitute is null || a.HasRequiredResidenceTitleOrVisa is null)
             return RuleResult.Success(
                 "Missing passport/visa facts. Cannot determine legality.",
                 new RuleReference("AufenthG", "§ 14", "Insufficient Data")
             );

        if (a.HasValidPassportOrSubstitute == false || a.HasRequiredResidenceTitleOrVisa == false)
        {
            return RuleResult.Failure(
                "Entry appears illegal due to missing passport/pass substitute and/or required title/visa (formal check).",
                new RuleReference("AufenthG", "§ 14", "Illegal Entry Indicator")
            );
        }

        return RuleResult.Success(
            "No indicators for illegal entry found in provided facts (formal check).",
            new RuleReference("AufenthG", "§ 14", "Check")
        );
    }
}
