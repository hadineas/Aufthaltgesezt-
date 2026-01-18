using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalCheck.API.DTOs;
using LegalCheck.Domain;
using LegalCheck.Domain.Laws.StAG;
using LegalCheck.Laws.AufenthG;

namespace LegalCheck.API.Services;

public sealed class DomainRuleService : IRuleRegistry, IRuleEvaluator
{
    // Hardcoded rule mapping for now. In real app, consider Reflection or DI scanning.
    private readonly Dictionary<string, List<IRule<LegalCaseContext>>> _rules = new();

    public DomainRuleService()
    {
        // AufenthG Setup
        var aufenthRules = new List<IRule<LegalCaseContext>>
        {
            new AufenthG_05_Abs1_Rule()
        };
        _rules.Add("AufenthG", aufenthRules);

        // StAG Setup
        var stagRules = new List<IRule<LegalCaseContext>>
        {
            new StAG_10_Rule()
        };
        _rules.Add("StAG", stagRules);
    }

    public IReadOnlyList<RuleInfoDto>? ListRules(string lawId)
    {
        if (!_rules.ContainsKey(lawId)) return null;

        return _rules[lawId].Select(r => new RuleInfoDto(
            RuleId: r.GetType().Name,
            Title: r.Reference.ToString(),
            Coverage: new CitationDto(r.Reference.LawId, r.Reference.NormId, r.Reference.UnitId, null, null, null),
            Status: "Implemented"
        )).ToList();
    }

    public IReadOnlyList<string>? ResolveRuleIds(string lawId, EvaluateRequestDto request)
    {
        if (!_rules.ContainsKey(lawId)) return null;
        
        // If specific rules requested, return them (validation logic omitted for brevity)
        if (request.RuleIds != null && request.RuleIds.Any()) return request.RuleIds;

        // Default: return all rules for this law
        return _rules[lawId].Select(r => r.GetType().Name).ToList();
    }

    public Task<IReadOnlyList<RuleResultDto>?> EvaluateAsync(string lawId, EvaluateRequestDto request)
    {
        if (!_rules.ContainsKey(lawId)) return Task.FromResult<IReadOnlyList<RuleResultDto>?>(null);

        // 1. Adapter: Map DTO -> Domain Context
        var applicant = MapApplicant(request.Context);
        var context = new LegalCaseContext(applicant);

        // 2. Resolve Rules
        var targetRuleIds = ResolveRuleIds(lawId, request);
        var rulesToRun = _rules[lawId].Where(r => targetRuleIds.Contains(r.GetType().Name));

        // 3. Evaluate
        var results = new List<RuleResultDto>();
        foreach (var rule in rulesToRun)
        {
            var result = rule.Evaluate(context);
            
            var citations = result.References.Select(r => 
                new CitationDto(r.LawId, r.NormId, r.UnitId, null, null, null)).ToList();

            results.Add(new RuleResultDto(
                RuleId: rule.GetType().Name,
                Status: result.IsSatisfied ? "Satisfied" : "NotSatisfied",
                IsSatisfied: result.IsSatisfied,
                Reasons: result.Reasons,
                Citations: citations,
                Sources: Array.Empty<SourceRefDto>()
            ));
        }

        return Task.FromResult<IReadOnlyList<RuleResultDto>?>(results);
    }

    private Applicant MapApplicant(CaseContextDto dto)
    {
        // Default safe values if null
        // Needs "dummy" Title if none provided, or logic to handle "No Title"
        var title = new ResidenceTitle(ResidenceTitleType.Tourist, "Unknown", "Unknown"); 
        
        var lang = LanguageLevel.A1;
        if (!string.IsNullOrEmpty(dto.LanguageLevel))
        {
             Enum.TryParse(dto.LanguageLevel, out lang);
        }

        var app = new Applicant("API User", title, dto.ResidenceYears ?? 0, lang)
        {
            IsLivelihoodSecured = dto.IsLivelihoodSecured ?? false,
            HasCriminalRecord = dto.HasCriminalRecord ?? false,
            MonthlyNetIncome = dto.MonthlyNetIncome ?? 0,
            MonthlyHousingCost = dto.MonthlyHousingCost ?? 0
        };
        return app;
    }
}
