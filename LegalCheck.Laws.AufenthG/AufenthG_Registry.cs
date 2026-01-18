using System.Collections.Generic;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

/// <summary>
/// Registry to easily access all AufenthG rules.
/// </summary>
public static class AufenthG_Registry
{
    // Gatekeepers (Blockers)
    public static IFactualRule Precheck11_EntryBan => new AufenthG_11_PrecheckRule();
    
    // Border Entry (§ 13-15)
    public static IFactualRule Precheck13_BorderStatus => new AufenthG_13_BorderCrossingStatusRule();
    public static IFactualRule Precheck14_IllegalEntry => new AufenthG_14_IllegalEntryPrecheckRule();
    public static IFactualRule Precheck15_BorderRefusal => new AufenthG_15_ReturnAtBorderRule();

    // Distribution § 15a
    public static IFactualRule Fact15a_Distribution => new AufenthG_15a_DistributionStatusRule();

    // Education § 16-16f
    public static IFactualRule Precheck16a_Vocational => new AufenthG_16a_PrecheckRule();
    public static IFactualRule Precheck16b_Study => new AufenthG_16b_PrecheckRule();
    public static IFactualRule Precheck16c_Mobility => new AufenthG_16c_MobilityPrecheckRule();
    public static IFactualRule Precheck16d_Recognition => new AufenthG_16d_PrecheckRule();
    public static IFactualRule Precheck16e_Internship => new AufenthG_16e_PrecheckRule();
    public static IFactualRule Precheck16f_LanguageSchool => new AufenthG_16f_PrecheckRule();

    // Evidence
    public static IFactualRule Fact16b_MatriculationEvidence => new Study_MatriculationEvidenceRule();

    public static IFactualRule Precheck10_Asylum => new AufenthG_10_PrecheckRule();
    
    // Existence / Compliance
    public static IFactualRule Precheck12a_Exists => new AufenthG_12a_ExistsRule();
    public static IFactualRule Precheck12a_Compliance => new AufenthG_12a_ComplianceRule();
    
    // Conditions & Exclusions (Filters)
    public static IFactualRule Precheck12_Conditions => new AufenthG_12_ConditionsPrecheckRule();
    public static IFactualRule Precheck9c_Exclusions => new AufenthG_9c_PrecheckRule();

    public static IEnumerable<IFactualRule> AllPrechecks => new[]
    {
        Precheck11_EntryBan,
        Precheck13_BorderStatus,
        Precheck14_IllegalEntry,
        Precheck15_BorderRefusal,
        Fact15a_Distribution,
        
        Precheck10_Asylum,
        Precheck12a_Exists,
        Precheck12a_Compliance, 
        
        Precheck16a_Vocational,
        Precheck16b_Study,
        Fact16b_MatriculationEvidence,
        Precheck16c_Mobility,
        
        Precheck12_Conditions,
        Precheck9c_Exclusions
    };
}
