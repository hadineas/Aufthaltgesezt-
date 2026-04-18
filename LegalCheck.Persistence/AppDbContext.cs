using Microsoft.EntityFrameworkCore;
using LegalCheck.Domain;

namespace LegalCheck.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Person> Persons { get; set; }
    public DbSet<EvaluationRecord> EvaluationRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Person Configuration
        modelBuilder.Entity<Person>()
            .HasKey(p => p.PersonId);

        // EvaluationRecord Configuration
        modelBuilder.Entity<EvaluationRecord>()
            .HasKey(e => e.Id);
            
        modelBuilder.Entity<EvaluationRecord>()
            .HasOne<Person>()
            .WithMany()
            .HasForeignKey(e => e.PersonId);

        // Store complex types like lists as JSON or separate tables. 
        // For simplicity in this demo, we might rely on EF Core 8's primitive collection support or JSON columns if using Sqlite.
        // However, EmploymentCases, etc. are complex objects. 
        // We really should configure them as Owned Types or separate tables.
        
        // Simplified approach: Person has EmploymentCases which are entities?
        // Let's create a separate config for Person to handle the lists.
        modelBuilder.Entity<Person>().OwnsMany(p => p.EmploymentCases);
        modelBuilder.Entity<Person>().OwnsMany(p => p.EducationCases);
        modelBuilder.Entity<Person>().OwnsOne(p => p.CurrentResidenceTitle);
        modelBuilder.Entity<Person>().OwnsMany(p => p.ResidencePeriods);
    }
}
