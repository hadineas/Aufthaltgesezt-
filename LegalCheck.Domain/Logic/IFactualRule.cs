using System.Collections.Generic;

namespace LegalCheck.Domain;

/// <summary>
/// Represents a rule that checks factual conditions (flags, exclusions, pre-checks)
/// without making a final legal decision on a claim.
/// </summary>
public interface IFactualRule
{
    string RuleId { get; }
    string[] Citations { get; }
    RuleResult Evaluate(CaseContext ctx);
}
