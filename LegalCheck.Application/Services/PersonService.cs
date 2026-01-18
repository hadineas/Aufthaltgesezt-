using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LegalCheck.Application.Interfaces;
using LegalCheck.Domain;

namespace LegalCheck.Application.Services;

public class PersonService
{
    private readonly IPersonRepository _personRepo;

    public PersonService(IPersonRepository personRepo)
    {
        _personRepo = personRepo;
    }

    public async Task<Person> CreatePersonAsync(Guid ownerUserId, string name, string nationalityIso2)
    {
        var person = new Person(ownerUserId, name)
        {
            NationalityIso2 = nationalityIso2
        };
        await _personRepo.AddAsync(person);
        return person;
    }

    public async Task<Person?> GetPersonAsync(Guid userId, Guid personId)
    {
        var person = await _personRepo.GetByIdAsync(personId);
        if (person == null) return null;
        
        // Authorization check
        if (person.OwnerUserId != userId) 
        {
            // In a real app tailored exception or null
            throw new UnauthorizedAccessException("Not owner of this person profile.");
        }
        return person;
    }

    public async Task<List<Person>> ListMyPersonsAsync(Guid userId)
    {
        return await _personRepo.ListByOwnerIdAsync(userId);
    }

    public async Task AddResidencePeriodAsync(Guid userId, Guid personId, ResidencePeriod period)
    {
        var person = await GetPersonAsync(userId, personId);
        if (person == null) throw new KeyNotFoundException("Person not found");
        person.ResidenceHistory.Add(period);
        await _personRepo.UpdateAsync(person);
    }

    public async Task AddPermitAsync(Guid userId, Guid personId, ResidencePermit permit)
    {
        var person = await GetPersonAsync(userId, personId);
        if (person == null) throw new KeyNotFoundException("Person not found");
        person.Permits.Add(permit);
        await _personRepo.UpdateAsync(person);
    }

    public async Task AddEducationAsync(Guid userId, Guid personId, EducationRecord record)
    {
        var person = await GetPersonAsync(userId, personId);
        if (person == null) throw new KeyNotFoundException("Person not found");
        person.EducationHistory.Add(record);
        await _personRepo.UpdateAsync(person);
    }

    public async Task AddEmploymentAsync(Guid userId, Guid personId, EmploymentRecord record)
    {
        var person = await GetPersonAsync(userId, personId);
        if (person == null) throw new KeyNotFoundException("Person not found");
        person.EmploymentHistory.Add(record);
        await _personRepo.UpdateAsync(person);
    }

    public async Task<Person> AddDocumentAsync(Guid userId, Guid personId, EvidenceDocument doc)
    {
         var person = await GetPersonAsync(userId, personId);
         if (person == null) throw new KeyNotFoundException("Person not found");
         person.Documents.Add(doc);
         await _personRepo.UpdateAsync(person);
         return person;
    }
}
