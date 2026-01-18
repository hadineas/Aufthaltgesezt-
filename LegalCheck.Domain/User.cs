using System;
using System.Collections.Generic;

namespace LegalCheck.Domain;

public class User
{
    public Guid UserId { get; private set; }
    public string Email { get; private set; }
    public List<string> Roles { get; private set; } = new();
    public DateTimeOffset CreatedAt { get; private set; }

    public User(Guid userId, string email)
    {
        UserId = userId == Guid.Empty ? Guid.NewGuid() : userId;
        Email = email;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void AddRole(string role)
    {
        if (!Roles.Contains(role)) Roles.Add(role);
    }
}
