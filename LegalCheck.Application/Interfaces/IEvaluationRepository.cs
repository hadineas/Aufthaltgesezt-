using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LegalCheck.Domain;

namespace LegalCheck.Application.Interfaces;

public interface IEvaluationRepository
{
    Task AddRecordAsync(EvaluationRecord record);
    Task<List<EvaluationRecord>> GetHistoryByPersonAsync(Guid personId);
}
