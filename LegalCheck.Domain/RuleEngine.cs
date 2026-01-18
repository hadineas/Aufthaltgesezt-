using System.Collections.Generic;
using System.Linq;

namespace LegalCheck.Domain;

public class RuleResult
{
    public bool IsSatisfied { get; }
    public List<string> Reasons { get; }
    public List<RuleReference> References { get; }

    public RuleResult(bool isSatisfied, IEnumerable<string> reasons = null, IEnumerable<RuleReference> references = null)
    {
        IsSatisfied = isSatisfied;
        Reasons = reasons?.ToList() ?? new List<string>();
        References = references?.ToList() ?? new List<RuleReference>();
    }

    public static RuleResult Success(string reason, RuleReference reference = null)
    {
        return new RuleResult(true, new[] { reason }, reference != null ? new[] { reference } : null);
    }

    public static RuleResult Failure(string reason, RuleReference reference = null)
    {
        return new RuleResult(false, new[] { reason }, reference != null ? new[] { reference } : null);
    }
}

// Simplifying the interface to be strictly typed to CaseContext for this project
public interface IRule
{
    RuleResult Evaluate(CaseContext context);
    RuleReference Reference { get; }
}

public abstract class BaseRule : IRule
{
    public abstract RuleReference Reference { get; }
    public abstract RuleResult Evaluate(CaseContext context);
}

// Simple composite rule for logical AND
public class AndRule : IRule
{
    private readonly List<IRule> _rules;
    public RuleReference Reference { get; } 

    public AndRule(RuleReference reference, params IRule[] rules)
    {
        Reference = reference;
        _rules = rules.ToList();
    }

    public RuleResult Evaluate(CaseContext context)
    {
        var failedResults = new List<RuleResult>();
        var successResults = new List<RuleResult>();

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(context);
            if (!result.IsSatisfied)
            {
                failedResults.Add(result);
            }
            else
            {
                successResults.Add(result);
            }
        }

        if (failedResults.Any())
        {
            var reasons = failedResults.SelectMany(r => r.Reasons).Distinct();
            var refs = failedResults.SelectMany(r => r.References).Distinct();
            return new RuleResult(false, reasons, refs);
        }

        var successReasons = successResults.SelectMany(r => r.Reasons).Distinct();
        var successRefs = successResults.SelectMany(r => r.References).Distinct();
        return new RuleResult(true, successReasons, successRefs);
    }
}
