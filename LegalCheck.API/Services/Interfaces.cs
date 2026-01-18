using System.Collections.Generic;
using System.Threading.Tasks;
using LegalCheck.API.DTOs;

namespace LegalCheck.API.Services;

public interface ILawRepository
{
    IReadOnlyList<LawSummaryDto> ListLaws();
    LawMetadataDto? GetMetadata(string lawId);
    IReadOnlyList<SectionListItemDto>? ListSections(string lawId, string? query, int skip, int take);
    SectionDto? GetSection(string lawId, string sectionId);
    TextUnitDto? GetUnit(string lawId, string unitId);
}

public interface IRuleRegistry
{
    IReadOnlyList<RuleInfoDto>? ListRules(string lawId);
    IReadOnlyList<string>? ResolveRuleIds(string lawId, EvaluateRequestDto request);
}

public interface IRuleEvaluator
{
    Task<IReadOnlyList<RuleResultDto>?> EvaluateAsync(string lawId, EvaluateRequestDto request);
}
