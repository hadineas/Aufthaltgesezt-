using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_18c_SettlementRule : IFactualRule
{
    public string RuleId => "Settlement_18c_Niederlassung";
    public string[] Citations => new[] { "AufenthG §18c" };

    public RuleResult Evaluate(CaseContext ctx)
    {
         // 1. Check if person is a Fachkraft
         // Simplified: Assume if they held titles 18a, 18b they qualify as starting point.
         
         // We look at ResidenceHistory to count months with "Fachkraft" titles.
         // Titles: Fachkraft (18a, 18b), BlueCardEU (18g).
         
         int monthsWithFachkraft = 0;
         int monthsWithBlueCard = 0;
         bool hasBlueCardCurrent = ctx.CurrentResidenceTitleCode == "BlueCardEU";
         
         foreach(var p in ctx.ResidencePeriods)
         {
             if (p.TitleType == ResidenceTitleType.Fachkraft) 
                monthsWithFachkraft += CalculateDurationMonths(p, ctx.AsOfDate);
             if (p.TitleType == ResidenceTitleType.BlueCardEU)
                monthsWithBlueCard += CalculateDurationMonths(p, ctx.AsOfDate);
         }
         
         int totalFachkraftTime = monthsWithFachkraft + monthsWithBlueCard;

         // § 18c Abs. 2 : Blue Card Holders
         if (hasBlueCardCurrent)
         {
             // 27 months regular (A1 German) OR 21 months fast (B1 German)
             
             bool sufficientLanguageForFastTrack = (int)ctx.LanguageLevel >= (int)LanguageLevel.B1;
             int requiredMonths = sufficientLanguageForFastTrack ? 21 : 27; // and A1 check for 27 (omitted, assumed A1 min)
             
             if (monthsWithBlueCard >= requiredMonths) // § 18c Abs. 2 counts employment period, strictly speaking
             {
                 // Pension check also required (same months)
                 if (ctx.PensionContributionMonths >= requiredMonths)
                 {
                      return RuleResult.Success($"Blue Card holder eligible for settlement after {monthsWithBlueCard} months.",
                          new RuleReference("AufenthG", "§ 18c Abs. 2", "Blue Card Settlement"));
                 }
                 else
                 {
                     return RuleResult.Failure($"Blue Card time sufficient ({monthsWithBlueCard}>={requiredMonths}), but Pension months insufficient ({ctx.PensionContributionMonths}).",
                         new RuleReference("AufenthG", "§ 18c Abs. 2", "Pension Missing"));
                 }
             }
         }

         // § 18c Abs. 1 : General Skilled Workers
         // 3 Years (36 Months). Reduced to 2 Years (24 Months) if Domestic Degree
         int requiredGeneral = 36;
         if (ctx.HasGermanDegree) requiredGeneral = 24;

         if (totalFachkraftTime >= requiredGeneral)
         {
             if (ctx.PensionContributionMonths >= requiredGeneral) // usually aligned
             {
                 if ((int)ctx.LanguageLevel >= (int)LanguageLevel.B1)
                 {
                     return RuleResult.Success($"Skilled worker eligible for settlement after {totalFachkraftTime} months.",
                          new RuleReference("AufenthG", "§ 18c Abs. 1", "Skilled Settlement"));
                 }
                 else
                     return RuleResult.Failure("Language level B1 required for § 18c Abs. 1.", new RuleReference("AufenthG", "§ 18c Abs. 1", "Language"));
             }
         }

        return RuleResult.Failure(
            $"Not eligible for § 18c Settlement. Time: {totalFachkraftTime} (Req: 36/24/21). Pension: {ctx.PensionContributionMonths}.",
            new RuleReference("AufenthG", "§ 18c", "Requirements Not Met")
        );
    }
    
    private int CalculateDurationMonths(ResidencePeriod p, System.DateTimeOffset asOf)
    {
        var end = p.EndDate ?? System.DateOnly.FromDateTime(asOf.Date);
        // Simple approximation
        int days = end.DayNumber - p.StartDate.DayNumber;
        if (days < 0) return 0;
        return days / 30; 
    }
}
