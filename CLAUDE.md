# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

A Pluralsight guided lab teaching Clean Architecture via an ASP.NET Core 10 MVC Recipe Catalog app. The `main` branch is the "before" state (monolithic single project with a fat controller). The `solution` branch is the "after" state (4-layer clean architecture).

## Build and Run

```bash
dotnet build
dotnet run --project Pluralsight.CleanArchitecture.Web/
```

Solution file: `Pluralsight.CleanArchitecture.slnx` (modern `.slnx` format). No test projects exist.

## Architecture

### Current State (main branch)

Everything lives in `Pluralsight.CleanArchitecture.Web/`. The `RecipesController` is a fat controller containing EF Core data access, business validation, file I/O, and ViewModel mapping all inline. Entities use data annotations. DbContext is injected directly into the controller.

### Target State (solution branch) — 4 Layers

```
Web → Application → Domain
Infrastructure → Application → Domain
```

- **Domain** (`Pluralsight.CleanArchitecture.Domain/`): Rich entities with no framework dependencies. Business rules live here (e.g., `Recipe.UpdatePreparation()`). No data annotations.
- **Application** (`Pluralsight.CleanArchitecture.Application/`): Interfaces/contracts (`IRecipeRepository`, `ICategoryRepository`, `INotificationService`) and service implementations (`RecipeService`, `CategoryService`). Depends only on Domain.
- **Infrastructure** (`Pluralsight.CleanArchitecture.Infrastructure/`): EF Core DbContext, repository implementations, `FileNotificationService`. Depends on Application.
- **Web** (`Pluralsight.CleanArchitecture.Web/`): Thin controllers, views, view models, Program.cs (composition root). Depends on Application and Infrastructure.

Dependencies point **inward only**. Interfaces defined in Application, implementations in Infrastructure (Dependency Inversion).

### DI Registration Pattern

Each layer has its own `*ServiceRegistration.cs` extension method class (`ApplicationServiceRegistration`, `InfrastructureServiceRegistration`) called from `Program.cs`. Uses built-in ASP.NET Core DI only — no MediatR, AutoMapper, or other third-party frameworks.

## Key Files

- `docs/plan.md` — Comprehensive lab plan with 6 steps and ~19 tasks
- `Pluralsight.CleanArchitecture.Web/Controllers/RecipesController.cs` — The fat controller to refactor
- `Pluralsight.CleanArchitecture.Web/Data/RecipeCatalogDbContext.cs` — EF Core context with SQLite and seed data

## Tech Stack

- .NET 10 / ASP.NET Core 10 MVC
- Entity Framework Core with SQLite (`recipecatalog.db` committed to repo)
- Bootstrap + jQuery (frontend)
- No test framework

## Writing Style

- Use American English (e.g., "organize" not "organise", "center" not "centre", "minimize" not "minimise")
- Capitalize layer names when referring to architectural layers: Domain, Application, Infrastructure, Web (e.g., "the Application layer", "the Domain project"). Use lowercase for generic usage (e.g., "the web application", "the application works").
