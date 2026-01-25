using System;
using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

// § 19 - ICT Card
public sealed class AufenthG_19_ICTCardRule : IFactualRule
{
    public string RuleId => "Entitlement_19_ICTCard";
    public string[] Citations => new[] { "AufenthG §19" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        var emp = ctx.EmploymentCases.FirstOrDefault(e => e.IsICT);
        if (emp == null) return RuleResult.Failure("No ICT case declared.", new RuleReference("AufenthG", "§ 19", "Skip"));

        // 1. Role Verification
        if (emp.IctRole == ICTRole.None)
             return RuleResult.Failure("ICT Role not specified (Manager, Specialist, Trainee).", new RuleReference("AufenthG", "§ 19 Abs. 2", "Role Missing"));

        // 2. Prior Employment in Group (§ 19 Abs. 2 Nr. 2) -> at least 6 months
        if (emp.PriorGroupEmploymentMonths < 6)
             return RuleResult.Failure($"Prior employment {emp.PriorGroupEmploymentMonths} months < 6 months.", 
                 new RuleReference("AufenthG", "§ 19 Abs. 2 Nr. 2", "Prior Duration"));

        // 3. Duration of Transfer (§ 19 Abs. 2 Nr. 3) -> > 90 days.
        // Assuming JobDurationMonths tracks the transfer duration here.
        if (emp.JobDurationMonths < 3 && emp.PlannedMobilityDays <= 90) // Rough check if months or days used
             return RuleResult.Failure("Transfer duration must be > 90 days (otherwise Schengen/Mobility).", 
                 new RuleReference("AufenthG", "§ 19 Abs. 2 Nr. 3", "Too Short"));

        // 4. Return Guarantee (§ 19 Abs. 2 Nr. 4b)
        if (!emp.HasReturnGuarantee)
             return RuleResult.Failure("Missing return guarantee to non-EU unit.", 
                 new RuleReference("AufenthG", "§ 19 Abs. 2 Nr. 4b", "Return Guarantee"));

        // 5. Max Duration Caps (§ 19 Abs. 4)
        // Manager/Specialist: Max 3 years (36 months)
        // Trainee: Max 1 year (12 months)
        int maxMonths = (emp.IctRole == ICTRole.Trainee) ? 12 : 36;
        if (emp.JobDurationMonths > maxMonths)
             return RuleResult.Failure($"Transfer duration {emp.JobDurationMonths} exceeds limit of {maxMonths} months for role {emp.IctRole}.", 
                 new RuleReference("AufenthG", "§ 19 Abs. 4", "Max Duration Exceeded"));

        // 6. Trainee Specifics (§ 19 Abs. 3) - University Degree Required
        if (emp.IctRole == ICTRole.Trainee)
        {
            if (emp.Qualification != QualificationType.AcademicDegree)
                 return RuleResult.Failure("ICT Trainee requires university degree.", 
                     new RuleReference("AufenthG", "§ 19 Abs. 3 S. 2", "Trainee Qualification"));
        }
        else 
        {
            // Manager/Specialist: "Proves professional qualification" (§ 19 Abs. 2 Nr. 5)
            // Can be vocational or academic. 
            if (emp.Qualification == QualificationType.None)
                 return RuleResult.Failure("Role requires professional qualification.", 
                     new RuleReference("AufenthG", "§ 19 Abs. 2 Nr. 5", "Qualification Missing"));
        }

        return RuleResult.Success($"Entitlement to ICT Card ({emp.IctRole}) met.", new RuleReference("AufenthG", "§ 19", "Granted"));
    }
}
