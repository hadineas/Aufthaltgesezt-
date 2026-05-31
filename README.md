# LegalCheck — Regel-Engine für das deutsche Aufenthaltsgesetz (AufenthG)

**LegalCheck** ist eine in .NET 8 entwickelte Regel- und Fallprüfungs-Engine, die die rechtlichen Voraussetzungen des deutschen **Aufenthaltsgesetzes (AufenthG)** maschinell auswertet. Das System bildet einzelne Paragraphen des AufenthG sowie ergänzender Gesetze (z. B. **StAG** — Staatsangehörigkeitsgesetz) als prüfbare, komponierbare Regeln in C# ab und liefert für einen konkreten Sachverhalt (eine Person mit Aufenthaltshistorie, Asylstatus, Bildungs-/Beschäftigungsdaten und Nachweisdokumenten) eine begründete Ja/Nein-Entscheidung mit der jeweiligen gesetzlichen Fundstelle.

> Zielgruppe: Migrationsberatung, Legal-Tech-Plattformen, Forschung sowie alle, die aufenthaltsrechtliche Vorprüfungen reproduzierbar, transparent und testbar abbilden möchten.

---

## Inhaltsverzeichnis

1. [Ziel des Projekts](#1-ziel-des-projekts)
2. [Architekturüberblick](#2-architekturüberblick)
3. [Projektstruktur](#3-projektstruktur)
4. [Domänenmodell](#4-domänenmodell)
5. [Regel-Katalog (LegalCheck.Laws.AufenthG)](#5-regel-katalog-legalchecklawsaufenthg)
6. [API-Endpunkte](#6-api-endpunkte)
7. [CLI](#7-cli)
8. [Persistenz](#8-persistenz)
9. [Technologie-Stack](#9-technologie-stack)
10. [Build & Ausführen](#10-build--ausführen)
11. [Beispielablauf](#11-beispielablauf)
12. [Lizenz & Hinweis](#12-lizenz--hinweis)

---

## 1. Ziel des Projekts

Aufenthaltsrechtliche Prüfungen sind in Deutschland komplex: Eine einzige Erlaubnis (z. B. **§ 18g — Blaue Karte EU**) verweist auf mehrere Vorprüfungen (§§ 5, 9c, 10, 11), kombiniert sie mit Bildungs- und Berufsvoraussetzungen (§§ 16–19) und verlangt zusätzlich konkrete Nachweise (Hochschulzeugnis, Arbeitsvertrag, Sprachzertifikat).

LegalCheck verfolgt drei konkrete Ziele:

- **Transparenz** — Jede Entscheidung wird mit dem zugrunde liegenden Paragraphen, Absatz und Satz zitiert (`Citations`). Niemand muss erraten, *warum* eine Voraussetzung erfüllt oder nicht erfüllt ist.
- **Reproduzierbarkeit** — Identische Sachverhalte führen deterministisch zum gleichen Ergebnis; die Regeln sind als Code versioniert und testbar.
- **Komponierbarkeit** — Vorprüfungen (z. B. § 5 Abs. 1 Lebensunterhalt, § 11 Einreise- und Aufenthaltsverbot) lassen sich beliebig mit Erlaubnis-spezifischen Regeln (§ 16b Studium, § 18g Blaue Karte) zu vollständigen Prüfungspfaden kombinieren.

Konkrete Anwendungsfälle:

- Vorprüfung der Niederlassungserlaubnis (§ 9c)
- Eignung für die **Blaue Karte EU** (§ 18g) inklusive Gehaltsschwelle und Qualifikation
- Studienerlaubnis (§ 16b) inkl. Immatrikulationsnachweis
- **ICT-Karte** (§ 19) für unternehmensinterne Transfers
- Naturalisierung nach **§ 10 StAG** (inkl. „Turbo-Einbürgerung" bei Sprachniveau C1)
- Asyl-Sperrwirkungen (§§ 10, 11) und Wohnsitzauflage (§ 12a)

## 2. Architekturüberblick

LegalCheck folgt der **Clean Architecture**. Abhängigkeiten zeigen ausschließlich nach innen — die Domäne kennt keine Datenbank, kein ASP.NET, kein EF Core.

```
┌──────────────────────────────────────────────────────────────┐
│                    LegalCheck.API  (ASP.NET Core, Minimal)   │
│                    LegalCheck.CLI  (Konsole / Demo)          │
├──────────────────────────────────────────────────────────────┤
│         LegalCheck.Persistence  (EF Core + SQLite)           │
├──────────────────────────────────────────────────────────────┤
│         LegalCheck.Application  (Services, Use Cases)        │
├──────────────────────────────────────────────────────────────┤
│         LegalCheck.Laws.AufenthG  (Regel-Katalog, §-Regeln)  │
├──────────────────────────────────────────────────────────────┤
│         LegalCheck.Domain  (Entities, Value Objects,         │
│                              IFactualRule, CaseContext)      │
└──────────────────────────────────────────────────────────────┘
```

**Abhängigkeitsregeln** (durchgesetzt über `.csproj`-Verweise):

| Projekt                        | Referenziert                            |
|--------------------------------|------------------------------------------|
| `LegalCheck.Domain`            | — (keine Abhängigkeit)                  |
| `LegalCheck.Application`       | `Domain`                                 |
| `LegalCheck.Laws.AufenthG`     | `Domain`                                 |
| `LegalCheck.Persistence`       | `Application`                            |
| `LegalCheck.API`               | `Domain` + `Application` + `Persistence` |
| `LegalCheck.CLI`               | `Domain` (+ Regeln zur Demo)             |

## 3. Projektstruktur

```
LegalCheck/
├── LegalCheck.sln
├── LegalCheck.Domain/         # Entities, Enums, Interfaces (IFactualRule, RuleResult)
├── LegalCheck.Application/    # PersonService, EvaluationService, DocumentService, DTOs
├── LegalCheck.Laws.AufenthG/  # §-Regeln (AufenthG_5, _9c, _10, _11, _16b, _18g, _19, ...)
├── LegalCheck.Persistence/    # AppDbContext, EF-Repositories, In-Memory-LawRepository
├── LegalCheck.API/            # Minimal-API (REST + Swagger)
├── LegalCheck.CLI/            # Demo-Konsolen-App mit 40+ Testfällen
└── Aufthaltgesezt/            # ältere Prototyp-Variante (separates Repo)
```

## 4. Domänenmodell

Das Domänenmodell liegt vollständig in `LegalCheck.Domain` und ist persistenz- und framework-frei.

**Kern-Aggregate und Wert-Typen:**

- **`Person`** — Persistentes Aggregat: Staatsangehörigkeit, Asylprofil, Aufenthaltshistorie, Erlaubnisse, Bildungs-/Beschäftigungsfälle, Dokumente, Sprachniveau, Vorstrafen, Einreisesperren, Lebensunterhalt.
- **`CaseContext`** — Unveränderlicher Snapshot für die Regel-Auswertung. Entkoppelt die persistente `Person` vom Regel-Engine: Regeln arbeiten ausschließlich gegen `CaseContext`, nie direkt gegen Datenbank-Entities.
- **`ResidenceTitle`** (Record) — Aufenthaltstitel-Typ + Paragraph + Beschreibung (z. B. *Niederlassungserlaubnis*, *Blaue Karte EU*, *Studienaufenthalt*, *Duldung*).
- **`AsylumProfile`** — Status (Pending / Recognized / Rejected / Subsidiary / ...), Abschiebehindernis, Ausreisepflicht, Identitätsklärung.
- **`EducationPurposeCase`** — Studium (§ 16b), Studienmobilität (§ 16c), Forschungsmobilität (§§ 18e/18f); Zulassungsstatus, geplanter Aufenthalt, BA-Zustimmung.
- **`EmploymentCase`** — Berufsbezeichnung, ISCO-Code, Gehalt, Qualifikation, BA-Zustimmung, Rentenversicherung, ICT-Rolle, Mobilitätsart.
- **`ResidencePeriod`**, **`ResidencePermit`**, **`PermitConditions`** — Verlauf und Auflagen vergangener Aufenthalte.
- **`EvidenceDocument`** — Hochgeladene Nachweise (Immatrikulationsbescheinigung, Pass, Anerkennungsbescheid, …).
- **`EntryAttempt`**, **`DistributionProcedure15a`**, **`ResidenceObligation12a`** — Spezialfälle (Verteilverfahren, Wohnsitzauflage).

**Wichtige Enums:**

- `LanguageLevel` (A1–C2; B1 = Mindestniveau für Einbürgerung, C1 = „Turbo-Einbürgerung" nach 3 Jahren)
- `ResidenceTitleType`, `AsylumStatus`, `EvidenceType`, `QualificationType`, `EquivalenceStatus`, `EmployeeMobilityType`, `ICTRole`

**Zentrale Abstraktionen:**

```csharp
public interface IFactualRule
{
    string   RuleId    { get; }
    string[] Citations { get; }
    RuleResult Evaluate(CaseContext ctx);
}

public sealed record RuleResult(
    bool IsSatisfied,
    string[] Reasons,
    string[] References);
```

Jede Regel ist damit eine pure Funktion `CaseContext → RuleResult` — leicht zu testen, leicht zu komponieren.

## 5. Regel-Katalog (LegalCheck.Laws.AufenthG)

`LegalCheck.Laws.AufenthG` enthält über 23 Regel-Klassen, die jeweils einen Paragraphen oder Absatz des AufenthG abbilden. Sie implementieren alle `IFactualRule`.

**Gatekeeper-Vorprüfungen** (brechen bei Nichterfüllung die gesamte Prüfung ab):

| Paragraph | Regel                                  | Inhalt                                            |
|-----------|----------------------------------------|---------------------------------------------------|
| § 5       | `AufenthG_05_Abs1_Rule`                | Allgemeine Erteilungsvoraussetzungen              |
| § 9c      | `AufenthG_9c_PrecheckRule`             | Ausschlüsse Daueraufenthalt-EU                    |
| § 10      | `AufenthG_10_PrecheckRule`             | Sperrwirkung Asylverfahren                        |
| § 11      | `AufenthG_11_PrecheckRule`             | Einreise- und Aufenthaltsverbot                   |
| § 12      | `AufenthG_12_ConditionsPrecheckRule`   | Bedingungen / Auflagen                            |
| § 12a     | `AufenthG_12a_ExistsRule` / `Compliance` | Wohnsitzauflage und deren Einhaltung            |

**Bildungs- und Forschungs-Regeln** (§§ 16a–17): Studium, Studienmobilität, Sprachkurs, Studienbewerbung, Berufsausbildungssuche, Anerkennungspartnerschaft.

**Beschäftigungs-Regeln** (§§ 18, 19): Fachkräfte (§ 18b), Niederlassung Fachkraft (§ 18c), **Blaue Karte EU** (§ 18g) inkl. Gehaltsschwelle, ICT-Karte (§ 19), kurzfristige Mobilität (§§ 18e/18f/18h/18i).

**Nachweis-Regeln**: prüfen, ob die zu einem Sachverhalt erforderlichen `EvidenceDocument`s vorhanden und gültig sind (z. B. `Study_MatriculationEvidenceRule`).

Die statische Klasse **`AufenthG_Registry`** liefert alle Regeln als Factory und bündelt insbesondere `AllPrechecks` — die vollständige Liste aller Gatekeeper, die jeder Erlaubnisprüfung vorgeschaltet wird.

## 6. API-Endpunkte

Die API ist als **Minimal API** in `LegalCheck.API/Program.cs` aufgebaut. Swagger ist im Entwicklungsbetrieb aktiv.

| Methode | Route                                              | Zweck                                              |
|---------|----------------------------------------------------|----------------------------------------------------|
| `GET`   | `/api/v1/laws`                                     | Liste aller verfügbaren Regeln / Gesetze           |
| `POST`  | `/api/v1/persons`                                  | Person anlegen → liefert `PersonId`                |
| `GET`   | `/api/v1/persons`                                  | Personen des aktuellen Nutzers auflisten           |
| `POST`  | `/api/v1/persons/{personId}/residence`             | Aufenthaltszeitraum hinzufügen                     |
| `POST`  | `/api/v1/persons/{personId}/permits`               | Aufenthaltserlaubnis hinzufügen                    |
| `POST`  | `/api/v1/persons/{personId}/education`             | Bildungsfall hinzufügen                            |
| `POST`  | `/api/v1/persons/{personId}/employment`            | Beschäftigungsfall hinzufügen                      |
| `POST`  | `/api/v1/persons/{personId}/documents`             | Nachweisdokument hochladen (multipart/form-data)   |
| `POST`  | `/api/v1/laws/{lawId}/evaluate`                    | Regel gegen Person bzw. Ad-hoc-Kontext auswerten   |

> **Hinweis zur Authentifizierung:** Im aktuellen Demo-Stand wird ein fester Nutzer (`UserId = 11111111-…`) verwendet. Für den Produktivbetrieb ist die Anbindung an einen Identity-Provider vorgesehen.

## 7. CLI

`LegalCheck.CLI` ist eine **Demo-Konsole** mit über 40 vorbereiteten Testfällen. Sie demonstriert Regeln direkt, ohne die Web-API zu starten:

- Aufbau von `Applicant`-Objekten (Aufenthaltstitel, Sprachniveau, Vorstrafen, Asylhistorie)
- Direkte Instanziierung einzelner Regeln (`AufenthG_05_Abs1_Rule`, `StAG_10_Rule`, `AufenthG_9c_PrecheckRule`, …)
- Auswertung gegen `LegalCaseContext` bzw. `CaseContext` (Helper `BuildDemoContext`)
- Szenarien: glücklicher Fall, Asyl-Ablehnung, Einreisesperre, Auflagenverletzung, Bildungs- und Beschäftigungspfade, Blaue-Karte-Gehaltsschwellen, ICT-Rollen
- Farbig formatierte Ergebnisausgabe (pass / fail mit Begründung)
- Integrationstest des `StandardRuleEvaluator` (Orchestrator) über den vollen Vorprüfungs-Stack

## 8. Persistenz

**`AppDbContext`** verwendet **EF Core 8 mit SQLite** (`legalcheck.db`):

- `DbSet<Person> Persons` — Aggregat-Wurzel; *owned types* für `EmploymentCases`, `EducationCases`, `CurrentResidenceTitle`, `ResidencePeriods`
- `DbSet<EvaluationRecord> EvaluationRecords` — Historisierte Auswertungen mit FK auf `Person`
- Schema wird beim Start per `EnsureCreated()` erzeugt (kein klassisches Migrations-Verzeichnis)

**Repositories:**

- `ILawRepository` → `InMemoryLawRepository` (Regelkatalog ist statisch — kein DB-Zugriff nötig)
- `IPersonRepository` → `EfPersonRepository`
- `IEvaluationRepository` → `EfEvaluationRepository`

## 9. Technologie-Stack

- **.NET 8.0** (alle Projekte, `Nullable` + `ImplicitUsings` aktiviert)
- **ASP.NET Core Minimal API**
- **Entity Framework Core 8.0** + **Microsoft.EntityFrameworkCore.Sqlite**
- **Swashbuckle.AspNetCore 6.5** (Swagger / OpenAPI)
- **Clean Architecture** — Domain ↔ Application ↔ Persistence ↔ API/CLI

## 10. Build & Ausführen

**Voraussetzungen:** .NET 8 SDK.

```bash
# Lösung bauen
dotnet build LegalCheck.sln

# Web-API starten (http://localhost:5252, https://localhost:7167)
dotnet run --project LegalCheck.API

# CLI-Demo ausführen
dotnet run --project LegalCheck.CLI
```

Swagger UI: nach Start der API erreichbar unter `http://localhost:5252/swagger`.

Die SQLite-Datei `legalcheck.db` wird beim ersten Start automatisch im Arbeitsverzeichnis der API erzeugt.

## 11. Beispielablauf

Typischer Ablauf zur Prüfung einer **Blauen Karte EU (§ 18g)** für eine konkrete Person:

1. `POST /api/v1/persons` — Person anlegen (Name, Staatsangehörigkeit, Geburtsdatum)
2. `POST /api/v1/persons/{id}/employment` — Arbeitsvertrag erfassen (Berufsbezeichnung, Gehalt, ISCO-Code)
3. `POST /api/v1/persons/{id}/documents` — Hochschulzeugnis als `EvidenceDocument` hochladen
4. `POST /api/v1/laws/AufenthG_18g/evaluate?personId={id}` — Auswertung anstoßen

Antwort (vereinfacht):

```json
{
  "ruleId": "AufenthG_18g",
  "isSatisfied": false,
  "reasons": [
    "Gehaltsschwelle nicht erreicht: 41.000 € < 45.300 € (Regelschwelle 2024)."
  ],
  "references": [
    "§ 18g Abs. 1 Satz 1 Nr. 2 AufenthG"
  ]
}
```

## 12. Lizenz & Hinweis

Dieses Projekt dient der **technischen Prüfungsunterstützung** und ersetzt **keine Rechtsberatung**. Die abgebildeten Paragraphen werden mit größtmöglicher Sorgfalt gepflegt; verbindlich ist allein der amtliche Gesetzestext. Für eine rechtsverbindliche Entscheidung im Einzelfall ist stets die zuständige Ausländerbehörde bzw. eine zur Rechtsberatung befugte Person heranzuziehen.

— *Stand: Mai 2026*
