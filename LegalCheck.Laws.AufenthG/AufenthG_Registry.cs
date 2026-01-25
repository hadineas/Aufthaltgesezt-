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

    // Search § 17
    public static IFactualRule Fact17_1_TrainingSearch => new AufenthG_17_1_TrainingSearchRule();
    public static IFactualRule Fact17_2_StudyApplicant => new AufenthG_17_2_StudyApplicantRule();

    // Employment § 18
    public static IFactualRule Def18_3_Fachkraft => new AufenthG_18_FachkraftDefinitionRule();
    public static IFactualRule Fact18_2_General => new AufenthG_18_GeneralRequirementsRule();
    public static IFactualRule Fact18_2_5_Age => new AufenthG_18_AgePensionRule();
    
    // Entitlements
    public static IFactualRule Entitlement18a => new AufenthG_18a_VocationalEntitlementRule();
    public static IFactualRule Entitlement18b => new AufenthG_18b_AcademicEntitlementRule();
    public static IFactualRule Entitlement18g => new AufenthG_18g_BlueCardRule();
    public static IFactualRule Settlement18c => new AufenthG_18c_SettlementRule();
    
    // Mobility § 18e / 18f
    public static IFactualRule Fact18e_Mobility => new AufenthG_18e_MobilityRule();
    public static IFactualRule Fact18f_MobileResearcher => new AufenthG_18f_MobileResearcherRule();
    
    // Mobility § 18h / 18i
    public static IFactualRule Fact18h_BusinessMobility => new AufenthG_18h_BusinessMobilityRule();
    public static IFactualRule Fact18i_LongTermMobility => new AufenthG_18i_LongTermMobilityRule();
    
    // § 19 ICT
    public static IFactualRule Entitlement19_ICTCard => new AufenthG_19_ICTCardRule();

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
        
        Fact17_1_TrainingSearch,
        Fact17_2_StudyApplicant,

        Def18_3_Fachkraft,
        Fact18_2_General,
        Fact18_2_5_Age,
        
        Entitlement18a,
        Entitlement18b,
        Entitlement18g,
        Settlement18c,
        Fact18e_Mobility,
        Fact18f_MobileResearcher,
        Fact18h_BusinessMobility,
        Fact18i_LongTermMobility,
        Entitlement19_ICTCard,

        Precheck12_Conditions,
        Precheck9c_Exclusions
    };
}
