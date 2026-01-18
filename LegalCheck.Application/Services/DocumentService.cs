using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LegalCheck.Domain;

namespace LegalCheck.Application.Services;

public class DocumentService
{
    private readonly string _storageRoot;

    public DocumentService()
    {
        // For demo purposes, store in a local "storage" folder relative to execution
        _storageRoot = Path.Combine(AppContext.BaseDirectory, "storage");
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<EvidenceDocument> UploadDocumentAsync(Guid personId, string fileName, byte[] content, EvidenceType type)
    {
        // 1. Validation (Simulated)
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only PDF allowed.");

        if (content.Length == 0 || content.Length > 10 * 1024 * 1024)
            throw new ArgumentException("Invalid file size.");

        // 2. Compute Hash
        string sha256Hex;
        using (var sha = SHA256.Create())
        {
            var hashBytes = sha.ComputeHash(content);
            sha256Hex = Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        // 3. Store File
        var docId = Guid.NewGuid();
        var safeKey = $"persons_{personId}_docs_{docId}.pdf"; // Flattened path for simplicity in demo
        var fullPath = Path.Combine(_storageRoot, safeKey);

        await File.WriteAllBytesAsync(fullPath, content);

        // 4. Create Entity
        var doc = new EvidenceDocument
        {
            Id = docId,
            PersonId = personId,
            Type = type,
            OriginalFileName = fileName,
            ContentType = "application/pdf",
            SizeBytes = content.Length,
            StorageKey = safeKey,
            Sha256 = sha256Hex,
            UploadedAtUtc = DateTimeOffset.UtcNow
        };

        return doc;
    }
}
