using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LegalCheck.Domain;

namespace LegalCheck.Application.Interfaces;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(Guid personId);
    Task<List<Person>> ListByOwnerIdAsync(Guid ownerUserId);
    Task AddAsync(Person person);
    Task UpdateAsync(Person person);
}

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task AddAsync(User user);
}
