using System;
using System.Collections.Generic;
using System.Linq;

namespace LegalCheck.Domain.Logic;

public sealed record LongTermEuResidenceSummary(
    int CountableMonthsTotal,
    int CountableMonthsInGermany,
    int CountableMonthsAbroad,
    string[] Notes);

public static class AufenthG9bCalculator
{
    // NO LEGAL ADVICE. Technical Pre-calculation based on Title and Period data.
    public static LongTermEuResidenceSummary ComputeCountableMonths(
        IReadOnlyList<ResidencePeriod> periods,
        IReadOnlyList<ResidencePermit> permits,
        DateOnly asOf)
    {
        var notes = new List<string>();

        if (periods is null || periods.Count == 0)
            return new LongTermEuResidenceSummary(0, 0, 0, new[] { "No residence periods provided." });

        int deMonths = 0;
        int abroadMonths = 0;

        foreach (var p in periods)
        {
            var start = p.StartDate;
            var end = p.EndDate ?? asOf;
            if (end < start) continue;
            if (end > asOf) end = asOf;

            var months = ApproxMonths(start, end);

            // Germany time: usually countable if lawful
            if (string.Equals(p.CountryIso2, "DE", StringComparison.OrdinalIgnoreCase))
            {
                if (p.IsLawful == false)
                {
                    notes.Add($"Excluded DE period {start}: marked not lawful.");
                    continue;
                }
                
                // If IsLawful is null, we assume True for this demo or strictly require it. 
                // Let's assume True if typically entering valid periods, or warn.
                if (p.IsLawful == null)
                {
                     // implicit assumption or check permit
                }

                deMonths += months;
                continue;
            }

            // Abroad time: potentially countable per § 9b
            // Requires holding a German title during that time (simplified)
            var hadTitle = p.HadGermanTitleDuringPeriod 
                           ?? HadAnyPermitCovering(permits, start, end);

            if (!hadTitle)
            {
                notes.Add($"Excluded abroad period {p.CountryIso2} ({start}): no German title during period.");
                continue;
            }

            if (p.CountsForLongTermEu == false)
            {
                notes.Add($"Excluded abroad period {p.CountryIso2}: marked not countable.");
                continue;
            }

            abroadMonths += months;
        }

        return new LongTermEuResidenceSummary(
            CountableMonthsTotal: deMonths + abroadMonths,
            CountableMonthsInGermany: deMonths,
            CountableMonthsAbroad: abroadMonths,
            Notes: notes.ToArray());
    }

    private static bool HadAnyPermitCovering(IReadOnlyList<ResidencePermit> permits, DateOnly start, DateOnly end)
    {
        if (permits is null) return false;
        // Check if any permit covers the ENTIRE period or AT LEAST overlaps?
        // § 9b typically requires holding the title "during" the stay. 
        // Simplification: Check for overlap
        return permits.Any(x => x.IssuedAt <= start && x.ValidUntil >= end);
    }

    private static int ApproxMonths(DateOnly start, DateOnly end)
    {
        var days = end.DayNumber - start.DayNumber;
        return Math.Max(0, (int)Math.Floor(days / 30.4375));
    }
}
