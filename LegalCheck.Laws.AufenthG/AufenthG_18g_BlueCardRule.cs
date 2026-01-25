using System;
using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 18g - Blue Card EU
public sealed class AufenthG_18g_BlueCardRule : IFactualRule
{
    public string RuleId => "Entitlement_18g_BlueCard";
    public string[] Citations => new[] { "AufenthG §18g" };

    // 2024 Thresholds (Demo approximation)
    // BBG 2024 (West) ~ 7550 EUR/month
    // 50% = 3775 EUR
    // 45.3% = 3420 EUR
    private const decimal ThresholdStandard = 3775m; 
    private const decimal ThresholdReduced = 3420m;

    public RuleResult Evaluate(CaseContext ctx)
    {
        var emp = ctx.EmploymentCases.FirstOrDefault(e => e.HasConcreteJobOffer);
        if (emp == null) return RuleResult.Failure("No job offer found.", new RuleReference("AufenthG", "§ 18g", "Job Offer Missing"));

        // 1. Minimum Duration Check (§ 18g Abs. 3)
        if (emp.JobDurationMonths < 6)
            return RuleResult.Failure($"Job duration {emp.JobDurationMonths} months too short (Min 6).", new RuleReference("AufenthG", "§ 18g Abs. 3", "Duration"));

        // logic flow:
        // A. Is "Fachkraft with Academic Training"? (§ 18g Abs. 1 S. 1)
        //    -> Requires Academic Degree + Recognized/German
        
        // B. Is "IT Specialist" (§ 18g Abs. 2)?
        //    -> Requires No Degree (or irrelevant), 3y Experience, ISCO 133/25
        
        bool isAcademic = (emp.Qualification == QualificationType.AcademicDegree && (emp.IsGermanDegree || emp.Equivalence == EquivalenceStatus.Confirmed));
        bool isItSpecialist = emp.HasITSpecialistExperience; // Simplified input flag

        if (!isAcademic && !isItSpecialist)
             return RuleResult.Failure("Neither Academic Degree nor IT Specialist requirement met.", new RuleReference("AufenthG", "§ 18g", "Qualification"));

        // PATH A: ACADEMIC
        if (isAcademic)
        {
            // A1. Standard Threshold (50%)
            if (emp.MonthlySalaryGross >= ThresholdStandard)
                return RuleResult.Success("Blue Card (Standard) requirements met.", new RuleReference("AufenthG", "§ 18g Abs. 1 S. 1", "Standard"));

            // A2. Reduced Threshold (45.3%)
            // Check if Shortage Occupation OR Fresh Graduate
            bool isShortage = IsShortageOccupation(emp.IscoCode);
            bool isFreshGrad = IsFreshGraduate(emp.GraduationDate, ctx.AsOfDate);

            if (isShortage || isFreshGrad)
            {
                if (emp.MonthlySalaryGross >= ThresholdReduced)
                {
                    string reason = isShortage ? "Shortage Occupation" : "Recent Graduate";
                    return RuleResult.Success($"Blue Card ({reason}) requirements met (Reduced Threshold).", 
                        new RuleReference("AufenthG", "§ 18g Abs. 1 S. 2", "Reduced"));
                }
                else
                {
                    return RuleResult.Failure($"Salary {emp.MonthlySalaryGross} below reduced threshold {ThresholdReduced}.", 
                        new RuleReference("AufenthG", "§ 18g Abs. 1 S. 2", "Salary Too Low"));
                }
            }
            return RuleResult.Failure($"Salary {emp.MonthlySalaryGross} below standard threshold {ThresholdStandard} and not eligible for reduction.", 
                new RuleReference("AufenthG", "§ 18g Abs. 1 S. 1", "Salary Too Low"));
        }

        // PATH B: IT SPECIALIST (No Degree required)
        if (isItSpecialist)
        {
            // Needs ISCO 133 or 25
            if (!IsItIsco(emp.IscoCode))
                return RuleResult.Failure($"IT Specialist requires ISCO 133 or 25 (Has: {emp.IscoCode}).", new RuleReference("AufenthG", "§ 18g Abs. 2", "Bad ISCO"));

            if (emp.MonthlySalaryGross >= ThresholdReduced)
                 return RuleResult.Success("Blue Card (IT Specialist) requirements met.", new RuleReference("AufenthG", "§ 18g Abs. 2", "IT"));
            else
                 return RuleResult.Failure($"IT Specialist Salary {emp.MonthlySalaryGross} below threshold {ThresholdReduced}.", new RuleReference("AufenthG", "§ 18g Abs. 2", "Salary Too Low"));
        }

        return RuleResult.Failure("Fallthrough failure.", new RuleReference("AufenthG", "§ 18g", "Unknown"));
    }

    private bool IsShortageOccupation(string isco)
    {
        // § 18g Abs. 1 S. 2 Nr. 1: Groups 132, 133, 134, 21, 221, 222, 225, 226, 23, 25
        if (string.IsNullOrWhiteSpace(isco) || isco.Length < 2) return false;
        
        // Exact prefix match logic
        string[] prefixes = { "132", "133", "134", "21", "221", "222", "225", "226", "23", "25" };
        return prefixes.Any(p => isco.StartsWith(p));
    }

    private bool IsItIsco(string isco)
    {
        // § 18g Abs. 2: Groups 133 or 25
         if (string.IsNullOrWhiteSpace(isco)) return false;
         return isco.StartsWith("133") || isco.StartsWith("25");
    }

    private bool IsFreshGraduate(DateOnly? gradDate, DateTimeOffset asOf)
    {
        // § 18g Abs. 1 S. 2 Nr. 2: Not more than 3 years before application
        if (gradDate == null) return false;
        
        var limit = DateOnly.FromDateTime(asOf.Date).AddYears(-3);
        return gradDate >= limit;
    }
}
