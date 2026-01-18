using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LegalCheck.Application.Interfaces;
using LegalCheck.Domain;

namespace LegalCheck.Persistence.Repositories;

public class InMemoryPersonRepository : IPersonRepository
{
    private readonly Dictionary<Guid, Person> _store = new();

    public Task<Person?> GetByIdAsync(Guid personId)
    {
        _store.TryGetValue(personId, out var person);
        return Task.FromResult(person);
    }

    public Task<List<Person>> ListByOwnerIdAsync(Guid ownerUserId)
    {
        var list = _store.Values.Where(p => p.OwnerUserId == ownerUserId).ToList();
        return Task.FromResult(list);
    }

    public Task AddAsync(Person person)
    {
        _store[person.PersonId] = person;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Person person)
    {
        _store[person.PersonId] = person;
        return Task.CompletedTask;
    }
}
