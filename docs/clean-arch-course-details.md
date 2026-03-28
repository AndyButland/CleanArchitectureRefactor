# ASP.NET Core Clean Architecture - Course Summary

**Course:** ASP.NET Core Clean Architecture (Pluralsight)
**Author:** Gill Cleeren
**Target Framework:** ASP.NET Core 8 (.NET 6-9 compatible)

---

## Course Overview

The course builds a real-world **GloboTicket Ticket Management API** from scratch using clean architecture principles in ASP.NET Core 8. The learner plays a consultant hired by GloboTicket to establish an application architecture for their new projects.

---

## Transformation from Poor to Clean Architecture

The course explicitly covers a progression of architecture styles (Module 2):

1. **"All-in-One" / File-New-Project architecture** - Everything in a single project, layers enforced only by folders. Described as unmaintainable at scale.
2. **Traditional Layered architecture** - Presentation -> Business Logic -> Data Access. Better separation, but layers are still tightly coupled (Business Logic depends on Data Access).
3. **Clean Architecture (Onion Architecture)** - Concentric circles where the core has zero knowledge of outer layers. Dependencies point inward only.

The course also shows a **controller transformation**: heavy controllers -> view services -> MediatR-based lightweight controllers, progressively reducing coupling.

---

## Module-by-Module Breakdown

### Module 1: Course Introduction
- GloboTicket scenario (legacy Web Forms company modernizing to .NET 8)
- Finished application demo (Blazor frontend + API backend with Swagger)

### Module 2: Foundational Architectural Principles
- **Dependency Inversion** (D in SOLID) - abstractions/interfaces to decouple
- **Separation of Concerns** (S in SOLID) - splitting functionality into discrete blocks
- **Single Responsibility** - one reason to change per class/layer
- **DRY (Don't Repeat Yourself)**
- **Persistence Ignorance** - domain entities as POCOs, no EF Core attributes
- Architecture styles comparison: All-in-One -> Layered -> Clean Architecture
- Clean architecture explained: concentric circles, core agnostic of infrastructure

### Module 3: Setting Up the Application Core
- **Business requirements gathering** (wireframes, entity identification)
- **Domain project** - POCO entities (Event, Category, Order), AuditableEntity base class
- **Application project** (core business logic):
  - **Contracts/Interfaces** - IAsyncRepository (generic), IEventRepository, ICategoryRepository, IOrderRepository
  - **Repository Pattern** - generic + entity-specific repositories
  - **MediatR** (Mediator Pattern) - IRequest messages + IRequestHandler handlers for loose coupling
  - **AutoMapper** - mapping entities to ViewModels, profiles
  - **CQRS** (Command Query Responsibility Segregation) - separating read (Queries) from write (Commands) into distinct classes
  - **Feature-based organization** - vertical slices (Features/Events, Features/Categories, Features/Orders) each with Commands and Queries folders
  - **Fluent Validation** - AbstractValidator classes, lambda-based rules, custom validators (including async database uniqueness checks)
  - **Custom Exceptions** - NotFoundException, BadRequestException, ValidationException
  - **Custom Response Types** - BaseResponse with Success, Message, ValidationErrors properties

### Module 4: Creating the Infrastructure Project
- **Persistence project** (EF Core):
  - DbContext with DbSets, OnModelCreating, seed data
  - Entity configurations (Fluent API, not data annotations)
  - SaveChangesAsync override for automatic audit tracking (CreatedDate, LastModifiedDate)
  - BaseRepository implementation + entity-specific repositories
  - Service registration extension methods
- **Infrastructure project**:
  - Email sending via **SendGrid** (IEmailService contract in Core, implementation in Infrastructure)
  - CSV export via **CsvHelper** (ICsvExporter contract in Core, implementation in Infrastructure)
  - Pattern: define interface in Core -> implement in Infrastructure -> register via DI

### Module 5: Adding the API (ASP.NET Core)
- API project setup with references to Core and Infrastructure
- **StartupExtensions** class (mimicking old Startup.cs)
- Service collection registration chaining (AddApplicationServices, AddInfrastructureServices, AddPersistenceServices)
- **Controller design progression**: heavy controllers -> view services -> **MediatR-based lightweight controllers** (recommended)
- EF Core migrations, database reset on startup
- **Return type strategy**: specific ViewModels for lists vs. details, custom response wrappers
- **Swagger/Swashbuckle** - API documentation and testing UI
- CORS configuration for client-side apps

### Module 6: Testing the Application Code
- **Unit Tests** (xUnit + Moq + Shouldly):
  - Testing query handlers (GetCategoriesListQueryHandler)
  - Testing command handlers (CreateCategoryCommandHandler)
  - Mocking repositories and AutoMapper
- **Integration Tests**:
  - DbContext tests with EF Core **InMemoryDatabase**
  - Testing SaveChangesAsync audit behavior
  - API controller tests using **WebApplicationFactory**
  - Testing full request pipeline with in-memory database
- **Functional Tests** - mentioned but not implemented (UI testing frameworks)

### Module 7: Adding the Blazor UI
- Blazor WebAssembly overview (Client, Server, Full Stack hosting models)
- **NSwag/NSwagStudio** - generating client-side C# (or TypeScript) code from Swagger spec
- Generated ServiceClient class with all API methods and DTO types
- Data services pattern in Blazor (IEventDataService -> EventDataService -> generated Client)
- **End-to-end feature addition** demo: adding paging (API -> Application -> Persistence -> NSwag regeneration -> Blazor)

### Module 8: Improving Application Behavior (Cross-Cutting Concerns)
- **Error Handling**:
  - Custom exceptions in Core (NotFoundException, BadRequestException, ValidationException)
  - **Custom middleware** (ExceptionHandlerMiddleware) to convert exceptions to HTTP status codes
  - Middleware pipeline configuration
- **Logging**:
  - ASP.NET Core built-in logging (ILogger<T>)
  - **Serilog** integration (file sink, structured logging, rolling intervals)
  - Configuration via appsettings.json
- **Authentication**:
  - ASP.NET Core 8 **Identity API endpoints** (replacing scaffolded Razor Pages)
  - Cookie-based authentication for SPA
  - Separate **Identity infrastructure project** with its own DbContext
  - Authorize attribute on controllers
  - Blazor: custom AuthenticationStateProvider, CookieHandler, login/register/logout pages

---

## Key Libraries/Packages Used

| Library | Purpose |
|---------|---------|
| **MediatR** | Mediator pattern / CQRS |
| **AutoMapper** | Object-to-object mapping |
| **FluentValidation** | Business rule validation |
| **Entity Framework Core** | Data persistence |
| **Serilog** | Structured logging |
| **Swashbuckle** | Swagger/OpenAPI docs |
| **NSwag** | Client code generation |
| **SendGrid** | Email infrastructure |
| **CsvHelper** | CSV export |
| **Moq** | Mocking framework |
| **Shouldly** | Assertion library |
| **xUnit** | Test framework |

---

## Lab Design Opportunities

Key hands-on exercises that could be built from this material:

1. **Before/after transformation** - Start with an all-in-one project, refactor to clean architecture
2. **Building the core** - Create domain, contracts, MediatR handlers, CQRS organization
3. **Infrastructure implementation** - Implement repository contracts with EF Core
4. **Adding a new feature end-to-end** - The email and CSV export demos are great lab exercises
5. **Testing** - Write unit tests with mocks, integration tests with InMemoryDatabase
6. **Cross-cutting concerns** - Add middleware for error handling, Serilog for logging, Identity for auth
7. **Paging feature** - The end-to-end paging addition touches every layer and makes an excellent capstone exercise
