using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalCheck.Application.Services;
using LegalCheck.Laws.AufenthG;

namespace LegalCheck.Domain.Logic;

public class StandardRuleEvaluator : IRuleEvaluator
{
    private readonly List<IFactualRule> _prechecks;

    public StandardRuleEvaluator()
    {
        // Load default registry. In a real app this might be injected.
        _prechecks = AufenthG_Registry.AllPrechecks.ToList();
    }

    public Task<EvaluationResult> EvaluateAsync(string lawId, CaseContext context)
    {
        var result = new EvaluationResult();
        
        // 1. Run Prechecks / Gatekeepers
        foreach (var check in _prechecks)
        {
            var r = check.Evaluate(context);
            result.RuleResults.Add(r);

            // Gatekeeper Logic:
            // If § 11 (Entry Ban) OR § 10 (Asylum Block) fail -> STOP.
            // We identify them by ID for now, or could have a property "IsGatekeeper" on interface.
            
            if (!r.IsSatisfied)
            {
                if (check.RuleId == "PRECHECK_AufenthG_11" || check.RuleId == "PRECHECK_AufenthG_10")
                {
                    // BLOCK! Do not process further rules.
                    return Task.FromResult(result);
                }
                
                // § 14 / § 15 Logic: If Refused at Border or Illegal Entry, potentially block?
                // The architecture says "Gatekeeper".
                // If § 15 Refusal occurred, this is a blocker for normal residence titles in many cases.
                if (check.RuleId == "PRECHECK_AufenthG_15_ReturnAtBorder" || check.RuleId == "PRECHECK_AufenthG_14_IllegalEntry")
                {
                    // Treat as blocker for this flow
                     return Task.FromResult(result);
                }
            }
        }

        // 2. Run Main Entitlement Rules (e.g. § 9a, § 18b, etc.)
        // Only if gatekeepers passed.
        // TODO: Orchestrate main rules based on 'lawId' requested.
        // For now, we only have prechecks implemented in this phase.
        
        return Task.FromResult(result);
    }
}
