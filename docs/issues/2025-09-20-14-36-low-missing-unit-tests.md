---
id: missing-unit-tests-001
severity: LOW
category: testing
language: csharp
created: 2025-09-20T14:36:00Z
status: open
---

# Issue: Complete Absence of Unit Tests

## Location
**Project**: `/data/repos/sheet-atlas/`
**Missing**: Test project and test files

## Violation
**Standard**: csharp-dotnet.md - Testing Standards
**Rule**: "Implement comprehensive unit testing"

## Description
Il progetto non include alcun test unitario o progetto di test, rendendo difficile verificare la correttezza del codice e prevenire regressioni durante le modifiche future.

## Current State
- Nessun progetto di test nella solution
- Nessuna classe di test
- Nessuna configurazione per test runner
- Mancanza di coverage testing

## Suggested Fix
**Struttura di testing da implementare**:

1. Creare progetto di test:
```bash
dotnet new xunit -n SheetAtlas.Tests
dotnet sln add SheetAtlas.Tests/SheetAtlas.Tests.csproj
```

2. Aggiungere pacchetti necessari:
- xUnit o NUnit per testing framework
- Moq per mocking
- FluentAssertions per assertions

3. Implementare test per:
- ExcelFileManager
- ConfigManager
- ExcelData models
- Validation logic

**Esempio struttura test**:
```
tests/
├── SheetAtlas.Tests.csproj
├── Managers/
│   ├── ExcelFileManagerTests.cs
│   └── ConfigManagerTests.cs
├── Models/
│   └── ExcelDataTests.cs
└── TestData/
    └── sample.xlsx
```

## Reasoning
I test unitari garantiscono:
- Verifica della correttezza funzionale
- Prevenzione di regressioni
- Facilitazione del refactoring
- Documentazione del comportamento atteso

## References
- Standard: csharp-dotnet.md - Testing Best Practices
- Standard: general-principles.md - Quality Assurance
- CLAUDE.md: Incrementality and testing principles