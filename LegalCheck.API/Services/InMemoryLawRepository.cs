using System;
using System.Collections.Generic;
using System.Linq;
using LegalCheck.API.DTOs;

namespace LegalCheck.API.Services;

public sealed class InMemoryLawRepository : ILawRepository
{
    private static readonly DateTimeOffset RetrievedAt = DateTimeOffset.UtcNow;

    public IReadOnlyList<LawSummaryDto> ListLaws() =>
        new[] 
        { 
            new LawSummaryDto("AufenthG", "Aufenthaltsgesetz"),
            new LawSummaryDto("StAG", "Staatsangehörigkeitsgesetz") 
        };

    public LawMetadataDto? GetMetadata(string lawId)
    {
        return lawId switch
        {
            "AufenthG" => new LawMetadataDto("AufenthG", "Aufenthaltsgesetz", "https://www.gesetze-im-internet.de/aufenthg_2004/", RetrievedAt, "1.0.0"),
            "StAG" => new LawMetadataDto("StAG", "Staatsangehörigkeitsgesetz", "https://www.gesetze-im-internet.de/stag/", RetrievedAt, "1.0.0"),
            _ => null
        };
    }

    public IReadOnlyList<SectionListItemDto>? ListSections(string lawId, string? query, int skip, int take)
    {
        if (lawId != "AufenthG" && lawId != "StAG") return null;

        var all = new List<SectionListItemDto>();

        if (lawId == "AufenthG")
        {
            all.Add(new("AufenthG:§5", "§ 5", "Allgemeine Erteilungsvoraussetzungen"));
        }
        else if (lawId == "StAG")
        {
            all.Add(new("StAG:§10", "§ 10", "Anspruchseinbürgerung"));
        }

        if (!string.IsNullOrWhiteSpace(query))
            all = all.Where(s => (s.Display + " " + s.Heading).Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        return all.Skip(Math.Max(0, skip)).Take(take).ToList();
    }

    public SectionDto? GetSection(string lawId, string sectionId)
    {
        // Mock implementation returning minimal data
        return new SectionDto(sectionId, "§ X", "Mock Section", new List<TextUnitDto>());
    }

    public TextUnitDto? GetUnit(string lawId, string unitId)
    {
        return null; // Mock
    }
}
