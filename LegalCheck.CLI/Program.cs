using System;
using System.Collections.Generic;
using LegalCheck.Domain;
using LegalCheck.Laws.AufenthG;
using LegalCheck.Domain.Logic;
using LegalCheck.Domain.Laws.StAG;

namespace LegalCheck.CLI;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("LegalCheck Demo - AufenthG § 5 Evaluation");
        Console.WriteLine("-------------------------------------------");

        // 1. Create a "Happy Path" Case
        var safeApplicant = new Applicant("Mustermann", 
            new ResidenceTitle(ResidenceTitleType.BlueCardEU, "UNKNOWN", "Blue Card"), 
            5, 
            LanguageLevel.B1)
        {
            IsLivelihoodSecured = true,
            HasCriminalRecord = false
        };
        
        var contextSafe = new LegalCaseContext(safeApplicant);

        // 2. Create a "Reject" Case
        var criminalApplicant = new Applicant("Ganove", 
            new ResidenceTitle(ResidenceTitleType.Tourist, "UNKNOWN", "Tourist"), 
            1, 
            LanguageLevel.A1)
        {
            IsLivelihoodSecured = false,
            HasCriminalRecord = true
        };
        
        var contextCriminal = new LegalCaseContext(criminalApplicant);

        // 3. Instantiate Rule
        var aufenthRule = new AufenthG_05_Abs1_Rule();

        // 4. Evaluate
        EvaluateAndPrint(aufenthRule, contextSafe);
        EvaluateAndPrint(aufenthRule, contextCriminal);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - StAG § 10 (Naturalization)");
        Console.WriteLine("-------------------------------------------");

        // 5. Create Naturalization Candidate (Standard)
        var natCandidate = new Applicant("Einbuergerungswilliger",
            new ResidenceTitle(ResidenceTitleType.Niederlassungserlaubnis, "§ 26", "Settlement Permit"),
            8, // Years
            LanguageLevel.B1)
        {
            IsLivelihoodSecured = true,
            MonthlyNetIncome = 2500m,
            MonthlyHousingCost = 800m
        };
        
        // 6. Create "Turbo" Candidate (C1 Language, Short Residence)
        var turboCandidate = new Applicant("Turbo-Einbuergerung",
            new ResidenceTitle(ResidenceTitleType.BlueCardEU, "§ 18b", "Blue Card"),
            3, // Only 3 years!
            LanguageLevel.C1) // but C1
        {
            MonthlyNetIncome = 4000m,
            MonthlyHousingCost = 1000m,
            EntryDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-3).AddDays(-1)) // Precise date
        };

        // 7. Create Failed Candidate (Not enough Money)
        var poorCandidate = new Applicant("Zu Wenig Einkommen",
            new ResidenceTitle(ResidenceTitleType.Niederlassungserlaubnis, "§ 26", "Settlement"),
            10,
            LanguageLevel.B1)
        {
            MonthlyNetIncome = 800m, // < 563 + 500 rent
            MonthlyHousingCost = 500m
        };

        var stagRule = new StAG_10_Rule();

        EvaluateAndPrint(stagRule, new LegalCaseContext(natCandidate));
        EvaluateAndPrint(stagRule, new LegalCaseContext(turboCandidate));
        EvaluateAndPrint(stagRule, new LegalCaseContext(poorCandidate));

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - AufenthG § 9c (Precheck)");
        Console.WriteLine("-------------------------------------------");

        // 8. § 9c Test Case: Temporary Purpose
        var tempApplicant = new Applicant("Temporary Student", 
            new ResidenceTitle(ResidenceTitleType.Aufenthaltserlaubnis, "§ 16b", "Study"), 
            2, 
            LanguageLevel.B2);

        // Mutate permits manually since Applicant ctor is simple
        tempApplicant.Permits.Clear();
        tempApplicant.Permits.Add(new ResidencePermit(
            "§ 16b", 
            DateOnly.FromDateTime(DateTime.Now.AddYears(-2)), 
            DateOnly.FromDateTime(DateTime.Now.AddYears(1)), 
            "Study",
            IsTemporaryPurpose: true
        ));

        var contextTemp = new LegalCaseContext(tempApplicant);
        var rule9c = new AufenthG_9c_PrecheckRule();
        
        // We need an adapter or update EvaluateAndPrint to handle IFactualRule or convert
        // Since EvaluateAndPrint expects IRule<LegalCaseContext> and our new rule is IFactualRule (CaseContext)
        // We will just run it manually here for the demo to show the result
        
        // Note: IFactualRule takes CaseContext, not LegalCaseContext. Use adapter logic from Service.
        // We'll simulate the service mapping here:
        var domainContextTemp = BuildDemoContext(tempApplicant);

        var result9c = rule9c.Evaluate(domainContextTemp);
        PrintResult("§ 9c for Temporary Student", result9c);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - AufenthG § 10 (Asylum Gatekeeper)");
        Console.WriteLine("-------------------------------------------");

        // 9. § 10 Test Case: Pending Asylum (Should Fail)
        var pendingAsylumApplicant = new Applicant("Asylum Pending",
            new ResidenceTitle(ResidenceTitleType.Aufenthaltserlaubnis, "NONE", "None"),
            1, LanguageLevel.A1)
        {
            AsylumProfile = new AsylumProfile { Status = AsylumStatus.Pending }
        };
        var rule10 = new AufenthG_10_PrecheckRule();
        var result10a = rule10.Evaluate(BuildDemoContext(pendingAsylumApplicant));
        PrintResult("§ 10 for Pending Asylum", result10a);

        // 10. § 10 Test Case: Rejected & Deportable (Should Fail)
        var rejectedApplicant = new Applicant("Asylum Rejected",
             new ResidenceTitle(ResidenceTitleType.Duldung, "§60a", "Duldung"),
             3, LanguageLevel.A2)
        {
            AsylumProfile = new AsylumProfile 
            { 
                Status = AsylumStatus.Rejected,
                IsDeportable = true 
            }
        };
        var result10b = rule10.Evaluate(BuildDemoContext(rejectedApplicant));
        PrintResult("§ 10 for Rejected & Deportable", result10b);

        // 11. § 10 Test Case: Clean (Should Pass)
        var cleanApplicant = new Applicant("No Asylum History",
             new ResidenceTitle(ResidenceTitleType.BlueCardEU, "§18b", "Blue Card"),
             2, LanguageLevel.B2)
        {
            AsylumProfile = null
        };
        var result10c = rule10.Evaluate(BuildDemoContext(cleanApplicant));
        PrintResult("§ 10 for Clean History", result10c);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - AufenthG § 11 (Entry Ban)");
        Console.WriteLine("-------------------------------------------");

        // 12. § 11 Test Case: Active Ban (Future)
        var bannedApplicant = new Applicant("Banned Person", 
            new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 
            0, LanguageLevel.A1)
        {
            HasEntryBan = true,
            EntryBanUntil = DateOnly.FromDateTime(DateTime.Now.AddYears(5))
        };
        var rule11 = new AufenthG_11_PrecheckRule();
        var result11a = rule11.Evaluate(BuildDemoContext(bannedApplicant));
        PrintResult("§ 11 for Active Ban", result11a);

        // 13. § 11 Test Case: Expired Ban
        var priorBanApplicant = new Applicant("Prior Ban Person", 
            new ResidenceTitle(ResidenceTitleType.BlueCardEU, "§ 18b", "Blue Card"), 
            2, LanguageLevel.B1)
        {
            HasEntryBan = true,
            EntryBanUntil = DateOnly.FromDateTime(DateTime.Now.AddYears(-2))
        };
        var result11b = rule11.Evaluate(BuildDemoContext(priorBanApplicant));
        PrintResult("§ 11 for Expired Ban", result11b);

        var result11b = rule11.Evaluate(BuildDemoContext(priorBanApplicant));
        PrintResult("§ 11 for Expired Ban", result11b);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - AufenthG § 12 (Permit Conditions)");
        Console.WriteLine("-------------------------------------------");

        // 14. § 12 Test Case: Restricted Permit (Should Fail/Warn)
        var restrictedApplicant = new Applicant("Restricted Person", 
            new ResidenceTitle(ResidenceTitleType.Aufenthaltserlaubnis, "§ 18b", "Work"), 
            1, LanguageLevel.B1);

        // Add condition manually
        var restrictedPermit = new ResidencePermit("§ 18b", DateOnly.FromDateTime(DateTime.Now.AddYears(-1)), DateOnly.FromDateTime(DateTime.Now.AddYears(1)), "Work");
        restrictedPermit.Conditions.Add(new PermitCondition(PermitConditionType.SpatialRestriction, "Only Berlin"));
        
        restrictedApplicant.Permits.Clear();
        restrictedApplicant.Permits.Add(restrictedPermit);

        var rule12 = new AufenthG_12_ConditionsPrecheckRule();
        var result12a = rule12.Evaluate(BuildDemoContext(restrictedApplicant));
        PrintResult("§ 12 for Restricted Permit", result12a);

        var result12a = rule12.Evaluate(BuildDemoContext(restrictedApplicant));
        PrintResult("§ 12 for Restricted Permit", result12a);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - AufenthG § 12a (Residence Obligation)");
        Console.WriteLine("-------------------------------------------");

        // 15. § 12a Test Case: Obligation Exists
        var resObligationApplicant = new Applicant("Obligated Person", 
            new ResidenceTitle(ResidenceTitleType.HumanitarianProtection, "§ 25", "Refugee"), 
            1, LanguageLevel.A1)
        {
            ResidenceObligation12a = new ResidenceObligation12a 
            { 
                FederalStateCode = "DE-NW", // NRW
                ValidUntil = DateTimeOffset.Now.AddYears(2),
                IsActive = true
            }
        };

        var rule12aExists = new AufenthG_12a_ExistsRule();
        var result12aEx = rule12aExists.Evaluate(BuildDemoContext(resObligationApplicant));
        PrintResult("§ 12a Existence Check (Expected: NotSatisfied/Found)", result12aEx);
        
        // 16. § 12a Test Case: Compliance Check (Violation)
        // Assume default context builder sets CurrentResidence to "DE-UNK"
        var rule12aComp = new AufenthG_12a_ComplianceRule();
        var result12aComp = rule12aComp.Evaluate(BuildDemoContext(resObligationApplicant));
        PrintResult("§ 12a Compliance Check (Expected: Failure/Violation)", result12aComp);

        var rule12aComp = new AufenthG_12a_ComplianceRule();
        var result12aComp = rule12aComp.Evaluate(BuildDemoContext(resObligationApplicant));
        PrintResult("§ 12a Compliance Check (Expected: Failure/Violation)", result12aComp);

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - Evaluation Orchestrator (Full Flow)");
        Console.WriteLine("-------------------------------------------");

        // 17. Orchestration Test Case: Blocked by § 11 (should stop before other checks)
        // We reuse the 'bannedApplicant' who has an active entry ban.
        // We expect the Evaluator to return immediately after § 11 failure.
        
        var evaluator = new StandardRuleEvaluator();
        var bannedContext = BuildDemoContext(bannedApplicant);
        
        Console.WriteLine($"EVALUATING Full Flow for {bannedApplicant.Name} (Has Entry Ban)...");
        var bannedResult = evaluator.EvaluateAsync("AufenthG_05", bannedContext).Result; // Sync wait for demo
        
        foreach (var r in bannedResult.RuleResults)
        {
             PrintResult($"Rule {r.References.FirstOrDefault()?.NormId ?? "Unknown"}", r);
        }
        
        if (bannedResult.RuleResults.Count == 1 && bannedResult.RuleResults[0].Reasons.First().Contains("Entry ban"))
        {
             Console.ForegroundColor = ConsoleColor.Cyan;
             Console.WriteLine(">> VERIFIED: Evaluator stopped after § 11 failure.");
             Console.ResetColor();
        }

        // 18. Orchestration Test Case: Clean Case (should run all prechecks)
        // We reuse 'cleanApplicant'.
        // We expect all prechecks to run and pass.
        
        var cleanContext = BuildDemoContext(cleanApplicant);
        Console.WriteLine($"\nEVALUATING Full Flow for {cleanApplicant.Name} (Clean)...");
        var cleanResult = evaluator.EvaluateAsync("AufenthG_05", cleanContext).Result;

        foreach (var r in cleanResult.RuleResults)
        {
             var status = r.IsSatisfied ? "PASS" : "FAIL";
             var refId = r.References.FirstOrDefault()?.NormId ?? "Rule";
             Console.WriteLine($"  [{status}] {refId}");
        }
        Console.WriteLine($">> Total Rules Evaluated: {cleanResult.RuleResults.Count}");

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - Border Entry (§ 13, 14, 15)");
        Console.WriteLine("-------------------------------------------");

        // 19. Border Entry Test Case: Illegal Entry Attempt (§ 14)
        // Missing Visa/Passport
        var illegalEntrant = new Applicant("Illegal Entrant", 
            new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 0, LanguageLevel.A1)
        {
             LastEntryAttempt = new EntryAttempt
             {
                 AttemptedAt = DateTimeOffset.Now,
                 BorderPoint = "Forest Crossing",
                 Mode = BorderCrossingMode.Land,
                 IsAtDesignatedCrossingPoint = false, 
                 HasValidPassportOrSubstitute = false, // Illegal
                 HasRequiredResidenceTitleOrVisa = false, // Illegal
             }
        };

        var illegalContext = BuildDemoContext(illegalEntrant);
        Console.WriteLine($"Evaluating Illegal Entry for {illegalEntrant.Name}...");
        
        // Use Evaluator to see if it blocks
        var illegalResult = evaluator.EvaluateAsync("AufenthG_Border", illegalContext).Result;
        foreach (var r in illegalResult.RuleResults)
        {
             if (r.References.Any(x => x.NormId == "§ 14"))
             {
                 PrintResult("§ 14 Illegal Entry Check", r);
             }
        }
        
        // 20. Border Entry Test Case: Refusal at Border (§ 15)
        var refusedEntrant = new Applicant("Refused Person",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 0, LanguageLevel.A1)
        {
             LastEntryAttempt = new EntryAttempt
             {
                 AttemptedAt = DateTimeOffset.Now,
                 BorderPoint = "Airport MUC",
                 Mode = BorderCrossingMode.Air,
                 Decision = EntryDecisionType.Refused,
                 DecisionReason = "Security Concerns"
             }
        };
        var refusedContext = BuildDemoContext(refusedEntrant);
         Console.WriteLine($"\nEvaluating Refusal for {refusedEntrant.Name}...");
         var refusedResult = evaluator.EvaluateAsync("AufenthG_Border", refusedContext).Result;
         
         // Should fail at § 15
         foreach(var r in refusedResult.RuleResults)
         {
             if (r.References.Any(x => x.NormId == "§ 15"))
                PrintResult("§ 15 Refusal Check", r);
         }

         foreach(var r in refusedResult.RuleResults)
         {
             if (r.References.Any(x => x.NormId == "§ 15"))
                PrintResult("§ 15 Refusal Check", r);
         }

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - Education & Distribution (§ 15a - § 16f)");
        Console.WriteLine("-------------------------------------------");

        // 21. Distribution Test Case: Active Procedure (§ 15a)
        var distributedApplicant = new Applicant("Distributed Person",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 0, LanguageLevel.A1)
        {
             Distribution15a = new DistributionProcedure15a
             {
                 Status = DistributionStatus15a.Assigned,
                 AssignedFederalStateCode = "DE-BY", // Bavaria
                 DetectedAt = DateTimeOffset.Now.AddDays(-10)
             }
        };
        var distResult = evaluator.EvaluateAsync("AufenthG_Edu", BuildDemoContext(distributedApplicant)).Result;
        
        // Should show FACT
        foreach(var r in distResult.RuleResults)
        {
             if (r.References.Any(x => x.NormId == "§ 15a"))
                 PrintResult("§ 15a Distribution Status", r);
        }

        // 22. Education Test Case: Study § 16b (Missing Admission)
        var studentApplicant = new Applicant("Student Hopeful",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 1, LanguageLevel.B2);
             
        studentApplicant.EducationCases.Add(new EducationPurposeCase
        {
             PurposeType = EducationPurposeType.Study_16b,
             HasAdmissionOrContract = false // Missing!
        });
        
        var studentContext = BuildDemoContext(studentApplicant);
        var studentResult = evaluator.EvaluateAsync("AufenthG_Edu", studentContext).Result;
        
        // Should FAIL Precheck
        foreach(var r in studentResult.RuleResults)
        {
             if (r.References.Any(x => x.NormId == "§ 16b"))
                 PrintResult("§ 16b Study Precheck (Expected: Fail)", r);
        }

        // 23. Education Test Case: Mobility § 16c (Valid < 360 days)
        var mobilityApplicant = new Applicant("Mobile Student",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 1, LanguageLevel.C1);
             
        mobilityApplicant.EducationCases.Add(new EducationPurposeCase
        {
             PurposeType = EducationPurposeType.StudyMobility_16c,
             PlannedStayDays = 180 // Valid
        });
        
        var mobilityResult = evaluator.EvaluateAsync("AufenthG_Edu", BuildDemoContext(mobilityApplicant)).Result;
         // Should PASS Precheck
        foreach(var r in mobilityResult.RuleResults)
        {
             if (r.References.Any(x => x.NormId == "§ 16c"))
                 PrintResult("§ 16c Mobility Precheck (Expected: Pass)", r);
        }

        foreach(var r in mobilityResult.RuleResults)
        {
             if (r.References.Any(x => x.NormId == "§ 16c"))
                 PrintResult("§ 16c Mobility Precheck (Expected: Pass)", r);
        }

        Console.WriteLine("\n-------------------------------------------");
        Console.WriteLine("LegalCheck Demo - Evidence Documents (§ 16b)");
        Console.WriteLine("-------------------------------------------");

        // 24. Evidence Test Case: Study § 16b with Evidence
        var evidenceStudent = new Applicant("Student With Evidence",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 1, LanguageLevel.C1);
        
        evidenceStudent.EducationCases.Add(new EducationPurposeCase
        {
             PurposeType = EducationPurposeType.Study_16b,
             HasAdmissionOrContract = true
        });

        // Simulate upload
        // In real app, DocumentService.Upload would do this.
        evidenceStudent.Documents.Add(new EvidenceDocument
        {
             Type = EvidenceType.MatriculationCertificate,
             OriginalFileName = "Immatrikulationsbescheinigung_2024.pdf",
             SizeBytes = 500000,
             Sha256 = "dummyhash123",
             StorageKey = "simulated_path.pdf"
        });

        var evidenceResult = evaluator.EvaluateAsync("AufenthG_Edu", BuildDemoContext(evidenceStudent)).Result;
        
        // Should PASS and show Evidence Rule
        foreach(var r in evidenceResult.RuleResults)
        {
             if (r.RuleId == "FACT_16b_MatriculationEvidenceUploaded")
                 PrintResult("§ 16b Evidence Check (Expected: Yes)", r);
             
             if (r.RuleId == "PRECHECK_AufenthG_16b")
                 PrintResult("§ 16b General Precheck", r);
        }

        // 25. Evidence Test Case: Study § 16b WITHOUT Evidence
        var noEvidenceStudent = new Applicant("Student No Evidence",
             new ResidenceTitle(ResidenceTitleType.Tourist, "NONE", "None"), 1, LanguageLevel.C1);
             
        noEvidenceStudent.EducationCases.Add(new EducationPurposeCase
        {
             PurposeType = EducationPurposeType.Study_16b,
             HasAdmissionOrContract = true
        });

        var noEvidenceResult = evaluator.EvaluateAsync("AufenthG_Edu", BuildDemoContext(noEvidenceStudent)).Result;
        
        // Should FAIL Evidence Rule
        foreach(var r in noEvidenceResult.RuleResults)
        {
             if (r.RuleId == "FACT_16b_MatriculationEvidenceUploaded")
                 PrintResult("§ 16b Evidence Check (Expected: Missing)", r);
        }
    }

    static void PrintResult(string title, RuleResult result)
    {
        Console.WriteLine($"\nEvaluating {title}...");
         if (result.IsSatisfied)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[PASS] Satisfied");
        } 
        else 
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] Not Satisfied (Blocked)");
        }
        foreach (var reason in result.Reasons)
        {
            Console.WriteLine($"  -> {reason}");
        }
        Console.ResetColor();
    }

    // Quick helper to map Applicant -> CaseContext for CLI demo
    static CaseContext BuildDemoContext(Applicant p)
    {
        return new CaseContext(
             p.PersonId,
             DateTimeOffset.Now,
             p.NationalityIso2,
             false,
             null,
             0,
             false,
             p.Permits,
             p.ResidenceHistory,
             p.AsylumProfile,
             p.GermanLanguageLevel,
             false,
             true,
             p.IsLivelihoodSecured,
             p.MonthlyNetIncome ?? 0,
             p.MonthlyHousingCost,
             p.HasCriminalRecord,
             p.HasEntryBan,
             p.EntryBanUntil.HasValue 
                 ? new DateTimeOffset(p.EntryBanUntil.Value.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
                 : null,
             true,
             
             // § 12a
             p.ResidenceObligation12a,
             "DE-UNK" // Default unknown location for demo (will cause compliance fail if obligation exists)
        );
    }

    static void EvaluateAndPrint(IRule<LegalCaseContext> rule, LegalCaseContext context)
    {
        Console.WriteLine($"\nEvaluating {rule.Reference} for {context.Person.Name}...");
        var result = rule.Evaluate(context);

        if (result.IsSatisfied)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[PASS] {rule.Reference}: Satisfied");
            foreach (var reason in result.Reasons)
            {
                Console.WriteLine($"  + {reason}");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[FAIL] {rule.Reference}: Not Satisfied");
            foreach (var reason in result.Reasons)
            {
                Console.WriteLine($"  - {reason}");
            }
        }
        Console.ResetColor();
    }
}
