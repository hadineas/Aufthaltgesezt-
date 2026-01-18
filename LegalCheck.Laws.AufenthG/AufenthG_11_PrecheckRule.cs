using System;
using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class AufenthG_11_PrecheckRule : IFactualRule
{
    public string RuleId => "PRECHECK_AufenthG_11";
    public string[] Citations => new[] { "AufenthG §11" };

    public RuleResult Evaluate(CaseContext ctx)
    {
        if (ctx.HasEntryBan)
        {
            // If indefinite (null) or in the future -> BLOCK
            if (!ctx.EntryBanUntil.HasValue || ctx.EntryBanUntil.Value > ctx.AsOfDate)
            {
               var expiryText = ctx.EntryBanUntil.HasValue 
                   ? $"until {ctx.EntryBanUntil.Value:yyyy-MM-dd}" 
                   : "indefinitely";

               return RuleResult.Failure(
                   $"Active entry and residence ban (Einreise- und Aufenthaltsverbot) in effect {expiryText}.",
                   new RuleReference("AufenthG", "§ 11", "Entry Ban")
               );
            }
            
            // If expired
             return RuleResult.Success(
                 $"Entry ban existed but expired on {ctx.EntryBanUntil.Value:yyyy-MM-dd}.",
                 new RuleReference("AufenthG", "§ 11", "Entry Ban")
             );
        }

        return RuleResult.Success(
            "No entry ban recorded.", 
            new RuleReference("AufenthG", "§ 11", "Entry Ban")
        );
    }
}
