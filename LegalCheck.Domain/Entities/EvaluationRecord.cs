using System;

namespace LegalCheck.Domain;

public class EvaluationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PersonId { get; set; }
    public string RuleId { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string Citations { get; set; } // stored as comma sealed string
    public DateTimeOffset EvaluatedAt { get; set; }
}
