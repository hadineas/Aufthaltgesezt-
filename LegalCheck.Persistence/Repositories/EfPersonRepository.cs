using Microsoft.EntityFrameworkCore;
using LegalCheck.Domain;
using LegalCheck.Application; // Assumed interface location
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LegalCheck.Persistence.Repositories;

public class EfPersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;

    public EfPersonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Person?> GetByIdAsync(Guid personId)
    {
        // Must include owned types and collections eagerly if not auto-included
        // Owned types are usually auto-included by default in EF Core 8.
        return await _db.Persons
            .Include(p => p.EducationCases)
            .Include(p => p.EmploymentCases)
            //.Include(p => p.Permits) // These are lists ofOwned types? No, defined as lists of records in Person.cs
            // Records mapped as Owned Types need to be handled carefuly.
            // Assuming EF Core handles owned collections correctly.
            .FirstOrDefaultAsync(p => p.PersonId == personId);
    }

    public async Task SaveAsync(Person person)
    {
        if (_db.Entry(person).State == EntityState.Detached)
        {
            _db.Persons.Add(person);
        }
        else
        {
            _db.Persons.Update(person);
        }
        await _db.SaveChangesAsync();
    }
}
