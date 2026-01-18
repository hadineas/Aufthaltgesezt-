using System.Linq;
using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 16a - Vocational Training
public sealed class AufenthG_16a_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16a";
    public string[] Citations => new[] { "AufenthG §16a" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.VocationalTraining_16a);
        if (c is null) return RuleResult.Success("No §16a case data provided.", new RuleReference("AufenthG", "§ 16a", "Check"));

        var missing = new List<string>();
        if (c.HasAdmissionOrContract is null) missing.Add("HasAdmissionOrContract");
        if (c.RequiresBAApproval is null) missing.Add("RequiresBAApproval");
        if (c.RequiresBAApproval == true && c.HasBAApproval is null) missing.Add("HasBAApproval");

        if (missing.Count > 0)
            return RuleResult.Failure($"Missing facts: {string.Join(", ", missing)}", new RuleReference("AufenthG", "§ 16a", "Data Missing"));

        return RuleResult.Success(
            "§16a precheck facts captured. Full legal logic not fully implemented.",
             new RuleReference("AufenthG", "§ 16a", "Precheck")
        );
    }
}

// § 16b - Study
public sealed class AufenthG_16b_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16b";
    public string[] Citations => new[] { "AufenthG §16b" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.Study_16b);
        if (c is null) return RuleResult.Success("No §16b case data provided.", new RuleReference("AufenthG", "§ 16b", "Check"));

        if (c.HasAdmissionOrContract is null)
             return RuleResult.Failure("Missing fact: HasAdmissionOrContract (university admission).", new RuleReference("AufenthG", "§ 16b", "Data Missing"));

        if (c.HasAdmissionOrContract == false)
            return RuleResult.Failure("No admission recorded (formal precheck).", new RuleReference("AufenthG", "§ 16b", "Admission Missing"));

        return RuleResult.Success("Admission recorded. Full §16b logic not implemented.", new RuleReference("AufenthG", "§ 16b", "Precheck"));
    }
}

// § 16c - Mobility
public sealed class AufenthG_16c_MobilityPrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16c";
    public string[] Citations => new[] { "AufenthG §16c" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.StudyMobility_16c);
        if (c is null) return RuleResult.Success("No §16c mobility case data provided.", new RuleReference("AufenthG", "§ 16c", "Check"));

        if (c.PlannedStayDays is null)
             return RuleResult.Failure("Missing fact: PlannedStayDays.", new RuleReference("AufenthG", "§ 16c", "Data Missing"));

        if (c.PlannedStayDays > 360)
            return RuleResult.Failure($"Planned stay is {c.PlannedStayDays} days (> 360).", new RuleReference("AufenthG", "§ 16c", "Day Limit Exceeded"));

        return RuleResult.Success($"Planned stay is {c.PlannedStayDays} days (<= 360).", new RuleReference("AufenthG", "§ 16c", "Precheck"));
    }
}

// § 16d - Recognition
public sealed class AufenthG_16d_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16d";
    public string[] Citations => new[] { "AufenthG §16d" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.RecognitionMeasure_16d);
        if (c is null) return RuleResult.Success("No §16d recognition-measure case data provided.", new RuleReference("AufenthG", "§ 16d", "Check"));

        if (c.HasAdmissionOrContract is null)
             return RuleResult.Failure("Missing fact: HasAdmissionOrContract (recognition/qualification plan).", new RuleReference("AufenthG", "§ 16d", "Data Missing"));

        return c.HasAdmissionOrContract == true
            ? RuleResult.Success("Recognition measure recorded.", new RuleReference("AufenthG", "§ 16d", "Precheck"))
            : RuleResult.Failure("No recognition/qualification measure recorded.", new RuleReference("AufenthG", "§ 16d", "Missing Contract"));
    }
}

// § 16e - EU Internship
public sealed class AufenthG_16e_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16e";
    public string[] Citations => new[] { "AufenthG §16e" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.EUInternship_16e);
        if (c is null) return RuleResult.Success("No §16e internship case data provided.", new RuleReference("AufenthG", "§ 16e", "Check"));
        
        // Similar check to 16a
        if (c.HasAdmissionOrContract != true)
             return RuleResult.Failure("No internship agreement recorded.", new RuleReference("AufenthG", "§ 16e", "Missing Agreement"));
             
        return RuleResult.Success("§16e precheck facts captured.", new RuleReference("AufenthG", "§ 16e", "Precheck"));
    }
}

// § 16f - Language/School
public sealed class AufenthG_16f_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_16f";
    public string[] Citations => new[] { "AufenthG §16f" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var c = ctx.EducationCases.FirstOrDefault(x => x.PurposeType == EducationPurposeType.LanguageOrSchool_16f);
        if (c is null) return RuleResult.Success("No §16f case data provided.", new RuleReference("AufenthG", "§ 16f", "Check"));

        if (c.AdmissionType == AdmissionType.Unknown)
             return RuleResult.Failure("Missing AdmissionType.", new RuleReference("AufenthG", "§ 16f", "Data Missing"));

        if (c.HasAdmissionOrContract != true)
            return RuleResult.Failure("No enrollment recorded.", new RuleReference("AufenthG", "§ 16f", "Enrollment Missing"));

        return RuleResult.Success("Enrollment recorded.", new RuleReference("AufenthG", "§ 16f", "Precheck"));
    }
}
