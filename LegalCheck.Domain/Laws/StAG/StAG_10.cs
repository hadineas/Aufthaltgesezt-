using System;
using LegalCheck.Domain;

namespace LegalCheck.Domain.Laws.StAG;

// § 10 Staatsangehörigkeitsgesetz (StAG) - Anspruchseinbürgerung
// (1) Ein Ausländer, der seit fünf Jahren rechtmäßig seinen gewöhnlichen Aufenthalt im Inland hat...

public class StAG_10_Abs1_ResidenceDuration_Rule : BaseRule
{
    private const int StandardRequiredYears = 5;
    private const int AcceleratedRequiredYears = 3; 

    public override RuleReference Reference => new RuleReference("StAG", "§ 10", "Abs. 1 S. 1 (Aufenthaltsdauer)");

    public override RuleResult Evaluate(CaseContext context)
    {
        // 1. Check for "turbo" clause (C1 German level also allows 3 years)
        int requiredYears = StandardRequiredYears;
        if (context.LanguageLevel >= LanguageLevel.C1)
        {
            requiredYears = AcceleratedRequiredYears;
        }

        // Use pre-calculated residence months from context
        int actualYears = context.TotalResidenceMonths / 12;

        if (actualYears >= requiredYears)
        {
            return RuleResult.Success($"Aufenthaltsdauer von {actualYears} Jahren (ca. {context.TotalResidenceMonths} Monate) erfüllt (Erforderlich: {requiredYears}).", Reference);
        }

        return RuleResult.Failure($"Aufenthaltsdauer von {actualYears} Jahren zu kurz (Erforderlich: {requiredYears}).", Reference);
    }
}

public class StAG_10_Abs1_Nr3_Livelihood_Rule : BaseRule
{
    public override RuleReference Reference => new RuleReference("StAG", "§ 10", "Abs. 1 Nr. 3 (Lebensunterhalt)");

    public override RuleResult Evaluate(CaseContext context)
    {
        // "den Lebensunterhalt für sich und seine unterhaltsberechtigten Familienangehörigen ohne Inanspruchnahme von Leistungen... bestreiten kann"
        
        // 1. Direct check if flag is strictly set
        if (context.IsLivelihoodSecured)
        {
             return RuleResult.Success("Lebensunterhalt ist gesichert (Manuell bestätigt).", Reference);
        }

        // 2. Calculation Fallback (Example logic)
        // Assume minimum need = Housing + 563 EUR (Regelsatz 2024 for single)
        decimal regelsatz = 563m;
        decimal needs = context.MonthlyHousingCost + regelsatz;
        
        if (context.MonthlyNetIncome >= needs)
        {
            return RuleResult.Success($"Einkommen ({context.MonthlyNetIncome:C}) deckt Bedarf ({needs:C}).", Reference);
        }
        
        return RuleResult.Failure($"Lebensunterhalt nicht gesichert. Einkommen {context.MonthlyNetIncome:C} < Bedarf {needs:C}.", Reference);
    }
}

public class StAG_10_Rule : AndRule
{
    public StAG_10_Rule() : base(
        new RuleReference("StAG", "§ 10", "Allgemein"),
        new StAG_10_Abs1_ResidenceDuration_Rule(),
        new StAG_10_Abs1_Nr3_Livelihood_Rule()
        // Missing others like Identity, Criminal Record etc for simplicity of demo
    ) {}
}
