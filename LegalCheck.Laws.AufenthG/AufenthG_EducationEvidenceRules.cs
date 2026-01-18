using System.Collections.Generic;
using System.Linq;
using LegalCheck.Domain;

namespace LegalCheck.Laws.AufenthG;

public sealed class Study_MatriculationEvidenceRule : IFactualRule
{
    public string RuleId => "FACT_16b_MatriculationEvidenceUploaded";
    public string[] Citations => new[] { "AufenthG §16b (evidence)" };

    // This rule inspects the Documents collection in the context
    public RuleResult Evaluate(CaseContext ctx)
    {
        // Check if we have any MatriculationCertificate for the person
        var evidence = ctx.Documents.FirstOrDefault(d => d.Type == EvidenceType.MatriculationCertificate);

        if (evidence is not null)
        {
            return RuleResult.Success(
                $"Matrikulationsbescheinigung PDF uploaded: {evidence.OriginalFileName} ({evidence.SizeBytes} bytes).",
                new RuleReference("AufenthG", "§ 16b", "Evidence Provided")
            );
        }

        return RuleResult.Failure(
            "Missing Matrikulationsbescheinigung PDF.",
             new RuleReference("AufenthG", "§ 16b", "Evidence Missing")
        );
    }
}
