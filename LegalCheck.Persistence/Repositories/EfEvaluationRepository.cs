using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LegalCheck.Application.Interfaces;
using LegalCheck.Domain;

namespace LegalCheck.Persistence.Repositories;

public class EfEvaluationRepository : IEvaluationRepository
{
    private readonly AppDbContext _db;

    public EfEvaluationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddRecordAsync(EvaluationRecord record)
    {
        _db.EvaluationRecords.Add(record);
        await _db.SaveChangesAsync();
    }

    public async Task<List<EvaluationRecord>> GetHistoryByPersonAsync(Guid personId)
    {
        return await _db.EvaluationRecords
            .Where(e => e.PersonId == personId)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();
    }
}
