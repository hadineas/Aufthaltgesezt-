using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 5 Allgemeine Erteilungsvoraussetzungen

// Abs. 1
// Die Erteilung eines Aufenthaltstitels setzt in der Regel voraus, dass
// 1. der Lebensunterhalt gesichert ist,
// 1a. die Identität und, falls er nicht berechtigt ist, nach Deutschland zurückzukehren, die Staatsangehörigkeit des Ausländers geklärt ist,
// 2. kein Ausweisungsinteresse besteht,

public class AufenthG_05_Abs1_Nr1_Rule : BaseRule
{
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 5", "Abs. 1 Nr. 1");

    public override RuleResult Evaluate(CaseContext context)
    {
        if (context.IsLivelihoodSecured)
        {
            return RuleResult.Success("Lebensunterhalt ist gesichert.", Reference);
        }
        return RuleResult.Failure("Lebensunterhalt ist nicht gesichert.", Reference);
    }
}

public class AufenthG_05_Abs1_Nr2_Rule : BaseRule
{
    public override RuleReference Reference => new RuleReference("AufenthG", "§ 5", "Abs. 1 Nr. 2");

    public override RuleResult Evaluate(CaseContext context)
    {
        // Interpreting "HasCriminalRecord" as a proxy for "Ausweisungsinteresse" for simplicity in this demo.
        if (!context.HasCriminalRecord)
        {
            return RuleResult.Success("Kein Ausweisungsinteresse bekannt (Keine Vorstrafen).", Reference);
        }
        // In reality, criminal record != Ausweisungsinteresse automatically, but for the demo structure it suffices.
        return RuleResult.Failure("Ausweisungsinteresse könnte bestehen (Vorstrafen vorhanden).", Reference);
    }
}

public class AufenthG_05_Abs1_Rule : AndRule
{
    public AufenthG_05_Abs1_Rule() 
        : base(new RuleReference("AufenthG", "§ 5", "Abs. 1"), 
              new AufenthG_05_Abs1_Nr1_Rule(),
              new AufenthG_05_Abs1_Nr2_Rule())
    {
    }
}
