using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Domain.Laws.AufenthG;

// § 9a Erlaubnis zum Daueraufenthalt – EU
// (1) Die Erlaubnis zum Daueraufenthalt – EU ist ein unbefristeter Aufenthaltstitel...

public class AufenthG_09a_Abs2_Nr1_Residence_Rule : BaseRule
{
    // (2) Nr. 1: er sich seit fünf Jahren mit Aufenthaltstitel im Bundesgebiet aufhält
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 9a", "Abs. 2 Nr. 1");

    public override RuleResult Evaluate(CaseContext context)
    {
        int requiredMonths = 5 * 12; // 60 months
        if (context.TotalResidenceMonths >= requiredMonths)
        {
            return RuleResult.Success($"Aufenthaltsdauer erfüllt: {context.TotalResidenceMonths} Monate (Benötigt: {requiredMonths})", Reference);
        }
        return RuleResult.Failure($"Aufenthaltsdauer nicht erfüllt: {context.TotalResidenceMonths} Monate (Benötigt: {requiredMonths})", Reference);
    }
}

public class AufenthG_09a_Abs2_Nr2_Livelihood_Rule : BaseRule
{
    // (2) Nr. 2: sein Lebensunterhalt ... durch feste und regelmäßige Einkünfte gesichert ist
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 9a", "Abs. 2 Nr. 2");

    public override RuleResult Evaluate(CaseContext context)
    {
        // Using common 'IsLivelihoodSecured' flag or simple calc fallback
        if (context.IsLivelihoodSecured)
            return RuleResult.Success("Lebensunterhalt ist gesichert.", Reference);

        // Fallback check matching § 5 logic
        decimal regelsatz = 563m;
        decimal needs = context.MonthlyHousingCost + regelsatz;
        if (context.MonthlyNetIncome >= needs)
            return RuleResult.Success($"Einkommen ({context.MonthlyNetIncome:C}) deckt Bedarf ({needs:C}).", Reference);

        return RuleResult.Failure("Lebensunterhalt nicht gesichert.", Reference);
    }
}

public class AufenthG_09a_Abs2_Nr3_Language_Rule : BaseRule
{
    // (2) Nr. 3: er über ausreichende Kenntnisse der deutschen Sprache verfügt
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 9a", "Abs. 2 Nr. 3");

    public override RuleResult Evaluate(CaseContext context)
    {
        // "Ausreichende Kenntnisse" corresponds generally to B1 (CEFR)
        if (context.LanguageLevel >= LanguageLevel.B1)
        {
            return RuleResult.Success($"Sprachkenntnisse ausreichend ({context.LanguageLevel}).", Reference);
        }
        return RuleResult.Failure($"Sprachkenntnisse nicht ausreichend: {context.LanguageLevel} (Benötigt: B1).", Reference);
    }
}

public class AufenthG_09a_Abs2_Nr4_Integration_Rule : BaseRule
{
    // (2) Nr. 4: Grundkenntnisse der Rechts- und Gesellschaftsordnung
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 9a", "Abs. 2 Nr. 4");

    public override RuleResult Evaluate(CaseContext context)
    {
        // Usually proven by "Integration Course" OR "Einbürgerungstest" test logic
        // We use the new flag "IntegrationCourseCompleted" or "HasGermanDegree" as proxy
        
        if (context.IntegrationCourseCompleted || context.HasGermanDegree)
        {
             return RuleResult.Success("Grundkenntnisse vorhanden (Integrationskurs oder deutscher Abschluss).", Reference);
        }

        // Also simple declaration might suffice in some contexts, but let's be strict for 9a
        return RuleResult.Failure("Nachweis über Grundkenntnisse (Integrationskurs) fehlt.", Reference);
    }
}

public class AufenthG_09a_Abs3_Exclusion_Rule : BaseRule
{
    // (3) Absatz 2 ist nicht anzuwenden, wenn der Ausländer...
    // Nr. 4: sich mit einer Aufenthaltserlaubnis nach § 16a oder § 16b ... aufhält
    // Nr. 5: sich zu einem sonstigen seiner Natur nach vorübergehenden Zweck ... aufhält
    
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 9a", "Abs. 3 (Ausschlussgründe)");

    private static readonly string[] ExcludedTitles = new[] { "§ 16a", "§ 16b", "§ 16d", "§ 16e", "§ 16f", "§ 19c" }; // Simplified list

    public override RuleResult Evaluate(CaseContext context)
    {
        if (string.IsNullOrEmpty(context.CurrentResidenceTitleCode))
        {
             return RuleResult.Failure("Aktueller Aufenthaltstitel unbekannt - Prüfung auf Ausschlussgründe nicht möglich.", Reference);
        }

        // Check against exclude list
        // Note: Real world matching needs to handle spacing like "§16b" vs "§ 16b"
        string current = context.CurrentResidenceTitleCode.Replace(" ", "");
        
        foreach (var excluded in ExcludedTitles)
        {
            if (current.Contains(excluded.Replace(" ", "")))
            {
                return RuleResult.Failure($"§ 9a ist ausgeschlossen, da aktueller Titel ({context.CurrentResidenceTitleCode}) vorübergehender Natur ist ({excluded}).", Reference);
            }
        }

        return RuleResult.Success("Keine Ausschlussgründe nach Abs. 3 identifiziert.", Reference);
    }
}

public class AufenthG_09a_Rule : AndRule
{
    public AufenthG_09a_Rule() : base(
        new RuleReference("AufenthG", "§ 9a", "Erlaubnis zum Daueraufenthalt-EU"),
        new AufenthG_09a_Abs3_Exclusion_Rule(), // Check exclusions first!
        new AufenthG_09a_Abs2_Nr1_Residence_Rule(),
        new AufenthG_09a_Abs2_Nr2_Livelihood_Rule(),
        new AufenthG_09a_Abs2_Nr3_Language_Rule(),
        new AufenthG_09a_Abs2_Nr4_Integration_Rule()
    ) {}
}
