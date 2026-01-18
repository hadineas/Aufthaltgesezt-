using System;
using System.Collections.Generic;

namespace LegalCheck.API.DTOs;

public sealed record LawSummaryDto(string LawId, string Title);

public sealed record LawMetadataDto(
    string LawId,
    string Title,
    string SourceBaseUrl,
    DateTimeOffset RetrievedAtUtc,
    string GeneratorVersion);

public sealed record SectionListItemDto(
    string SectionId,          // "AufenthG:§5"
    string Display,            // "§ 5"
    string Heading);

public sealed record CitationDto(
    string LawId,
    string? Section,           // "§ 5"
    string? Absatz,            // "Abs. 1"
    string? Satz,              // "S. 1"
    string? Nummer,            // "Nr. 1"
    string? Buchstabe);        // "a"

public sealed record SourceRefDto(string Url, DateTimeOffset RetrievedAtUtc);

public sealed record TextUnitDto(
    string UnitId,             // "AufenthG:§5:Abs1:Nr1"
    CitationDto Citation,
    string Text,
    SourceRefDto Source);

public sealed record SectionDto(
    string SectionId,
    string Display,
    string Heading,
    IReadOnlyList<TextUnitDto> Units);

public sealed record RuleInfoDto(
    string RuleId,
    string Title,
    CitationDto Coverage,
    string Status); // "Stub" | "Implemented" | ...

public sealed record EvaluateRequestDto(
    CaseContextDto Context,
    IReadOnlyList<string>? RuleIds = null,
    string? Section = null);

public sealed record CaseContextDto(
    string? Nationality = null,
    // AufenthG fields
    bool? IsLivelihoodSecured = null,
    bool? HasCriminalRecord = null,
    // StAG fields
    int? ResidenceYears = null,
    string? LanguageLevel = null, // "B1", "C1" ...
    bool? IntegrationCourseCompleted = null,
    bool? CommitsToFdgo = null,
    decimal? MonthlyNetIncome = null,
    decimal? MonthlyHousingCost = null
    );

public sealed record RuleResultDto(
    string RuleId,
    string Status,              // "Satisfied" | "NotSatisfied" | "NotImplemented" | "InsufficientData"
    bool? IsSatisfied,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<CitationDto> Citations,
    IReadOnlyList<SourceRefDto> Sources);
