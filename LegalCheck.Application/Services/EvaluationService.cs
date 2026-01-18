using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LegalCheck.Application.Interfaces;
using LegalCheck.Domain;
using LegalCheck.Domain.Logic;

namespace LegalCheck.Application.Services;

public class EvaluationService
{
    private readonly IPersonRepository _personRepo;
    private readonly IRuleEvaluator _evaluator; // From Domain/API layer? Or define interface here? 
    private readonly IPersonRepository _personRepo;
    private readonly IRuleEvaluator _evaluator;

    public EvaluationService(IPersonRepository personRepo, IRuleEvaluator evaluator)
    {
        _personRepo = personRepo;
        _evaluator = evaluator;
    }

    public async Task<EvaluationResult> EvaluateForPersonAsync(
        Guid userId, Guid personId, string lawId, DateTimeOffset asOfDate)
    {
        var person = await _personRepo.GetByIdAsync(personId);
        if (person == null) throw new KeyNotFoundException("Person not found");

        if (person.OwnerUserId != userId)
            throw new UnauthorizedAccessException("Access denied");

        // Mapper: Person -> CaseContext
        var context = BuildContext(person, asOfDate);

        // Execute
        return await _evaluator.EvaluateAsync(lawId, context);
    }

    private CaseContext BuildContext(Person p, DateTimeOffset date)
    {
        var d = DateOnly.FromDateTime(date.DateTime);

        // 1. Calculate Active Title
        string? activeTitle = null;
        var currentPermit = p.Permits
            .Where(perm => perm.IssuedAt <= d && perm.ValidUntil >= d)
            .OrderByDescending(perm => perm.IssuedAt)
            .FirstOrDefault();
        if (currentPermit != null) activeTitle = currentPermit.PermitTypeCode;

        // 2. Calculate Total Residence Months (using § 9b Logic)
        var historySummary = AufenthG9bCalculator.ComputeCountableMonths(
            p.ResidenceHistory, 
            p.Permits, 
            d);
        
        int totalMonths = historySummary.CountableMonthsTotal;

        // 3. Current Income
        decimal income = 0;
        foreach (var emp in p.EmploymentHistory)
        {
            // Active employment?
            var end = emp.EndDate ?? d;
            if (emp.StartDate <= d && end >= d)
            {
                income += emp.MonthlyNetIncome;
            }
        }
        // If manual income set in person (legacy field removed? No, check Person.cs)
        // Person.cs monthly income removed? Let's check. 
        // Wait, I removed MonthlyNetIncome from Person.cs in previous step. 
        // So we strictly use aggregation now.

        // 4. German Degree?
        bool hasDegree = p.EducationHistory.Any(e => e.DidGraduate && e.Institution != null); // Simplistic check

        return new CaseContext(
            PersonId: p.PersonId,
            AsOfDate: date,
            NationalityIso2: p.NationalityIso2,
            IsEuCitizen: EuropeanUnion.IsMember(p.NationalityIso2),
            CurrentResidenceTitleCode: activeTitle,
            TotalResidenceMonths: totalMonths,
            HasGermanDegree: hasDegree,

            ResidencePermits: p.Permits,
            ResidencePeriods: p.ResidenceHistory,
            
            AsylumProfile: p.AsylumProfile,
            
            ResidenceObligation12a: p.ResidenceObligation12a,
            // Placeholder logic: In a real app, this would come from an Address Service or Person.Address
            CurrentResidenceAreaCode: "DE-UNK",
            
            CurrentEntryAttempt: p.LastEntryAttempt,
            
            Distribution15a: p.Distribution15a,
            EducationCases: p.EducationCases,
            
            Documents: p.Documents,

            LanguageLevel: p.GermanLanguageLevel,
            IntegrationCourseCompleted: p.IntegrationCourseCompleted,
            CommitsToFdgo: p.CommitsToFreeDemocraticOrder,
            IsLivelihoodSecured: p.IsLivelihoodSecured,
            MonthlyNetIncome: income,
            MonthlyHousingCost: p.MonthlyHousingCost,
            HasCriminalRecord: p.HasCriminalRecord,
            
            HasEntryBan: p.HasEntryBan,
            EntryBanUntil: p.EntryBanUntil.HasValue 
                ? new DateTimeOffset(p.EntryBanUntil.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) 
                : null,
            
            HasValidPassport: true // Placeholder
        );
    }
}

// Helpers
public interface IRuleEvaluator
{
    // Returns domain RuleResults
    Task<EvaluationResult> EvaluateAsync(string lawId, CaseContext context);
}

public class EvaluationResult
{
    public List<RuleResult> RuleResults { get; set; } = new();
}

public static class EuropeanUnion
{
    private static readonly HashSet<string> Members = new() { "DE", "FR", "IT", "ES", "PL", "NL", "BE", "SE", "AT", "DK", "FI", "IE", "PT", "GR", "CZ", "HU", "RO", "BG", "HR", "SK", "SI", "LT", "LV", "EE", "CY", "MT", "LU" };
    public static bool IsMember(string iso) => Members.Contains(iso.ToUpper());
}
