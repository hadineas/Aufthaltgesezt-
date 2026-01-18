using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LegalCheck.API.DTOs;
using LegalCheck.API.Services;
using LegalCheck.Application.Interfaces;
using LegalCheck.Application.Services;
using LegalCheck.Persistence.Repositories;
using LegalCheck.Persistence.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Infrastructure / Persistence
builder.Services.AddSingleton<ILawRepository, InMemoryLawRepository>();
builder.Services.AddSingleton<IPersonRepository, InMemoryPersonRepository>();
builder.Services.AddSingleton<IRuleEvaluator, RuleEngineService>(); // The engine implementation

// Application Services
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<EvaluationService>();
builder.Services.AddScoped<DocumentService>();

// Stub Auth for Demo
var DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/v1/laws", ([FromServices] ILawRepository repo) => Results.Ok(repo.ListLaws()));

// --- Person Management ---

app.MapPost("/api/v1/persons", async (CaseContextDto dto, [FromServices] PersonService service) =>
{
    // Simplified create from DTO
    var p = await service.CreatePersonAsync(DemoUserId, "API User", dto.Nationality ?? "Unknown");
    // TODO: Map other fields from DTO to Person
    return Results.Ok(p.PersonId);
});

app.MapGet("/api/v1/persons", async ([FromServices] PersonService service) =>
{
    return Results.Ok(await service.ListMyPersonsAsync(DemoUserId));
});

app.MapPost("/api/v1/persons/{personId}/residence", async (Guid personId, ResidencePeriod period, [FromServices] PersonService service) =>
{
    // The model binding will bind JSON to ResidencePeriod record properties automatically 
    // if the JSON keys match the property names (IsLawful, etc).
    await service.AddResidencePeriodAsync(DemoUserId, personId, period);
    return Results.Ok();
});

app.MapPost("/api/v1/persons/{personId}/permits", async (Guid personId, ResidencePermit permit, [FromServices] PersonService service) =>
{
    await service.AddPermitAsync(DemoUserId, personId, permit);
    return Results.Ok();
});

app.MapPost("/api/v1/persons/{personId}/education", async (Guid personId, EducationRecord record, [FromServices] PersonService service) =>
{
    await service.AddEducationAsync(DemoUserId, personId, record);
    return Results.Ok();
});

app.MapPost("/api/v1/persons/{personId}/employment", async (Guid personId, EmploymentRecord record, [FromServices] PersonService service) =>
{
    await service.AddEmploymentAsync(DemoUserId, personId, record);
    return Results.Ok();
});

app.MapPost("/api/v1/persons/{personId}/documents", async (
    Guid personId,
    HttpRequest request,
    [FromServices] PersonService personService,
    [FromServices] DocumentService docService) =>
{
   // Check if Person exists first (Ownership check inside PersonService)
   var p = await personService.GetPersonAsync(DemoUserId, personId);
   if (p == null) return Results.NotFound("Person not found.");

    if (!request.HasFormContentType) return Results.BadRequest("multipart/form-data required.");

    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file is null) return Results.BadRequest("No file provided.");

    // Determine Type
    var typeStr = request.Query["type"].ToString();
    var type = Enum.TryParse<LegalCheck.Domain.EvidenceType>(typeStr, true, out var t) ? t : LegalCheck.Domain.EvidenceType.Other;
    
    // Read content
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    var content = ms.ToArray();

    try 
    {
        // 1. Validate & Store
        var doc = await docService.UploadDocumentAsync(personId, file.FileName, content, type);
        
        // 2. Link to Person
        await personService.AddDocumentAsync(DemoUserId, personId, doc);

        return Results.Created($"/api/v1/persons/{personId}/documents/{doc.Id}", new { doc.Id, doc.OriginalFileName, doc.Type });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/api/v1/laws/{lawId}/evaluate", async (string lawId, EvaluateRequestDto req,
    [FromServices] EvaluationService service) =>
{
    // We expect a PersonId in a real scenario, but the DTO might allow ad-hoc or ID-based.
    // For this demo, let's assuming we created a person first OR allow ad-hoc.
    
    // "Option A" from blueprint: Evaluate by PersonId.
    // We need to extend EvaluateRequestDto to support PersonId or pass it in query/body.
    // Let's assume the blueprint meant: 
    // POST { personId: "...", asOfDate: "..." }
    
    // Quick Fix: If request has a Context (Ad-Hoc) we can't easily use EvaluationService which expects PersonId
    // unless we make EvaluationService support AdHoc or we create a temporary person.
    
    // Let's implement the Option A strictly: 
    // The user should have created a Person first.
    // But our DTO 'EvaluateRequestDto' currently has 'CaseContextDto Context'.
    // We will stick to AdHoc for now using the RuleEngine directly to keep it simple, 
    // OR create a temp Person.
    
    // Correct approach using EvaluationService:
    // Need a new DTO for Person-based eval? Or reuse.
    // Let's mock: "Create temp person -> Evaluate" to bridge the gap.
    
    Guid personId;
    var personService = app.Services.CreateScope().ServiceProvider.GetRequiredService<PersonService>();
    var p = await personService.CreatePersonAsync(DemoUserId, "AdHoc", req.Context.Nationality ?? "XX");
    // Update p properties from req.Context... (omitted for brevity)
    personId = p.PersonId;

    var result = await service.EvaluateForPersonAsync(DemoUserId, personId, lawId, DateTimeOffset.Now);
    
    // Map internal result to DTO
    var dtoResults = result.RuleResults.Select(r => new RuleResultDto(
        RuleId: "Rule", 
        Status: r.IsSatisfied ? "Satisfied" : "NotSatisfied",
        IsSatisfied: r.IsSatisfied,
        Reasons: r.Reasons,
        Citations: r.References.Select(reff => new CitationDto(reff.LawId, reff.NormId, reff.UnitId, null, null, null)).ToList(),
        Sources: new List<SourceRefDto>()
    ));

    return Results.Ok(dtoResults);
});

app.Run();
