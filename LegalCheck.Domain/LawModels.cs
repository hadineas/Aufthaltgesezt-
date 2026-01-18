using System.Collections.Generic;

namespace LegalCheck.Domain;

public record RuleReference(string LawId, string NormId, string UnitId)
{
    public override string ToString() => $"{LawId} {NormId} {UnitId}".Trim();
}

public class Law
{
    public string Id { get; }
    public string Name { get; }
    public List<Norm> Norms { get; } = new();

    public Law(string id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class Norm
{
    public string Id { get; } // e.g. "§ 5"
    public string Name { get; } // e.g. "Allgemeine Erteilungsvoraussetzungen"
    public List<Unit> Units { get; } = new();

    public Norm(string id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class Unit
{
    public string Id { get; } // e.g. "Abs. 1"
    public string Text { get; } 
    public List<Unit> SubUnits { get; } = new();

    public Unit(string id, string text)
    {
        Id = id;
        Text = text;
    }
}
