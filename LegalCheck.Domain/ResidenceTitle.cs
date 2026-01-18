namespace LegalCheck.Domain;

/// <summary>
/// Represents a specific legal residence title held by a person.
/// </summary>
public record ResidenceTitle(ResidenceTitleType Type, string ParagraphNumber, string Description)
{
    public bool IsPermanent => Type == ResidenceTitleType.Niederlassungserlaubnis || 
                               Type == ResidenceTitleType.ErlaubnisZumDaueraufenthaltEU;
}
