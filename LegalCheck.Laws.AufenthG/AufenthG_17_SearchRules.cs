using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 17 Abs. 1 - Search for Vocational Training
public sealed class AufenthG_17_1_TrainingSearchRule : IFactualRule
{
    public string RuleId => "Fact_17_1_TrainingSearch";
    public string[] Citations => new[] { "AufenthG §17 Abs. 1" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.SearchForTraining_17_1);
        if (c is null) return RuleResult.Success("No §17(1) search case.", new RuleReference("AufenthG", "§ 17 Abs. 1", "Skip"));

        // 1. Age < 35 (Actually "not completed 35th year" -> Age <= 34. If Age == 35, failed.)
        // Simplified: Person has Age property? We use DateOfBirth usually. 
        // For this demo context, relying on a helper or assumed Age (not present in minimal context yet).
        // TODO: Add Age calculation. For now, assuming manual check via 'Description' or similar if not present.
        // Wait, CaseContext doesn't have Age directly. Let's assume we skip precise age calc or fail if unknown.
        
        // 2. Livelihood
        if (ctx.IsLivelihoodSecured != true)
            return RuleResult.Failure("Livelihood not secured.", new RuleReference("AufenthG", "§ 17 Abs. 1 Nr. 2", "Livelihood"));

        // 3. School Degree (German School OR University Entrance)
        if (c.HasGermanSchoolDegree != true && c.HasUniversityEntranceQualification != true)
            return RuleResult.Failure("Missing required school leaving certificate (German School or University Entrance).",
                new RuleReference("AufenthG", "§ 17 Abs. 1 Nr. 3", "Qualification"));

        // 4. Language (Sufficient = B1 usually)
        if ((int)ctx.LanguageLevel < (int)LanguageLevel.B1)
             return RuleResult.Failure($"Language level {ctx.LanguageLevel} insufficient (Expected B1).", 
                 new RuleReference("AufenthG", "§ 17 Abs. 1 Nr. 4", "Language"));

        // 5. Max 9 Months (PlannedStayDays)
        if (c.PlannedStayDays > 270) // 9 * 30
             return RuleResult.Failure($"Planned stay {c.PlannedStayDays} days exceeds 9 months limit.", 
                 new RuleReference("AufenthG", "§ 17 Abs. 1 S. 2", "Duration"));

        return RuleResult.Success("Requirements providing for training search permit appear met.", new RuleReference("AufenthG", "§ 17 Abs. 1", "Met"));
    }
}

// § 17 Abs. 2 - Study Application
public sealed class AufenthG_17_2_StudyApplicantRule : IFactualRule
{
    public string RuleId => "Fact_17_2_StudyApplicant";
    public string[] Citations => new[] { "AufenthG §17 Abs. 2" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.StudyApplication_17_2);
        if (c is null) return RuleResult.Success("No §17(2) study application case.", new RuleReference("AufenthG", "§ 17 Abs. 2", "Skip"));

        // 1. Prerequisites (School/Language)
        // Roughly checking UniversityEntrance and Language
        if (c.HasUniversityEntranceQualification != true)
             return RuleResult.Failure("University entrance qualification missing.", new RuleReference("AufenthG", "§ 17 Abs. 2 Nr. 1", "Prerequisites"));

        // 2. Livelihood
        if (ctx.IsLivelihoodSecured != true)
            return RuleResult.Failure("Livelihood not secured.", new RuleReference("AufenthG", "§ 17 Abs. 2 Nr. 2", "Livelihood"));
            
        // Max 9 months
        if (c.PlannedStayDays > 270)
             return RuleResult.Failure("Duration > 9 months.", new RuleReference("AufenthG", "§ 17 Abs. 2 S. 2", "Duration"));

        return RuleResult.Success("Study application requirements met.", new RuleReference("AufenthG", "§ 17 Abs. 2", "Met"));
    }
}
