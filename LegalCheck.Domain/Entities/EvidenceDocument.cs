using System;

namespace LegalCheck.Domain;

public sealed class EvidenceDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }

    public EvidenceType Type { get; set; }
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = "application/pdf";
    public long SizeBytes { get; set; }

    // Store path/key, not the PDF itself (recommended)
    public string StorageKey { get; set; } = default!; // e.g. "persons/{personId}/docs/{id}.pdf"

    // Integrity
    public string Sha256 { get; set; } = default!;
    public DateTimeOffset UploadedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    // Optional extracted metadata
    public DateTimeOffset? DocumentIssuedAt { get; set; }
    public string? Notes { get; set; }
}
