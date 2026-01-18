using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalCheck.Application.Services; // For IRuleEvaluator
using LegalCheck.Domain;
using LegalCheck.Domain.Laws.StAG;
using LegalCheck.Laws.AufenthG;

namespace LegalCheck.Persistence.Services;

// Implementation of the evaluator used by Application layer
public class RuleEngineService : IRuleEvaluator
{
    private readonly Dictionary<string, List<IRule>> _rules = new();

    public RuleEngineService()
    {
        // AufenthG Setup
        var aufenthRules = new List<IRule>
        {
            new AufenthG_05_Abs1_Rule(),
            new AufenthG_09a_Rule()
        };
        _rules.Add("AufenthG", aufenthRules);

        // StAG Setup
        var stagRules = new List<IRule>
        {
            new StAG_10_Rule()
        };
        _rules.Add("StAG", stagRules);
    }

    public Task<EvaluationResult> EvaluateAsync(string lawId, CaseContext context)
    {
        var result = new EvaluationResult();
        
        if (_rules.TryGetValue(lawId, out var rules))
        {
            foreach (var rule in rules)
            {
                result.RuleResults.Add(rule.Evaluate(context));
            }
        }

        return Task.FromResult(result);
    }
}
