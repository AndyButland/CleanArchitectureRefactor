# Plan: ASP.NET Core Clean Architecture Lab

## Context

We're building a Pluralsight guided code lab (~1 hour) that teaches clean architecture principles through hands-on refactoring. The learner starts with a working but poorly-structured single-project API and progressively refactors it into a multi-project clean architecture solution.

The lab follows the exact same format as the existing Pluralsight.OrderForm lab: markdown with embedded Semgrep YAML checks, two git branches (`main` for starter code, `solution` for completed code), and step/task structure.

**Domain:** Recipe Catalog (simple, relatable, distinct from the course's GloboTicket)

**Concepts brief (docs/concepts.md):**
- T01: Analyze existing app to identify Clean Architecture violations
- T02: Refactor into domain, application, infrastructure, and presentation layers
- T03: Apply dependency inversion to decouple from infrastructure
- T04: Verify refactored architecture works correctly

---

## Deliverables

### 1. "Before" State (main branch)

A single-project ASP.NET Core 10 MVC application with everything crammed into one project. Uses EF Core with SQLite for data access and writes notifications directly to the filesystem — all from inside the controllers. Includes a full working UI with recipe list, filtering, and CRUD forms.

**Project structure:**
```
Pluralsight.CleanArchitecture.WebUi/
    Controllers/
        RecipesController.cs        # Fat controller: EF Core queries, File.AppendAllText,
                                    #   inline validation, duplicated mapping, CRUD actions
    Data/
        RecipeCatalogDbContext.cs    # DbContext with DbSets, in WebUi (wrong place)
    Models/
        Recipe.cs                   # Entity with data annotations
        Category.cs                 # Entity with data annotations
    ViewModels/                     # Presentation models — UNCHANGED during lab
        RecipeIndexViewModel.cs     # Recipe list + filter state (category, difficulty)
        RecipeFormViewModel.cs      # Create/Edit form (includes category dropdown items)
        RecipeDeleteViewModel.cs    # Delete confirmation (Id, Title)
    Views/                          # Razor views — UNCHANGED during lab
        Recipes/
            Index.cshtml            # Recipe list with category/difficulty filter dropdowns
            Create.cshtml           # Create recipe form
            Edit.cshtml             # Edit recipe form
            Delete.cshtml           # Delete confirmation page
        Shared/
            _Layout.cshtml          # Bootstrap layout, "Recipe Catalog" branding
            _ValidationScriptsPartial.cshtml
        _ViewStart.cshtml
        _ViewImports.cshtml
    wwwroot/                        # Bootstrap + jQuery (from template, same as OrderForm)
    Program.cs                      # Registers DbContext, Directory.CreateDirectory
    recipecatalog.db                # Pre-seeded SQLite database (committed)
```

**Key characteristics:**
- A pre-seeded `recipecatalog.db` SQLite file ships with the project (4 categories, 8 recipes). A `seed.sql` script is included in the project root for reference and reproducibility.
- `RecipeCatalogDbContext` lives directly in the WebUi project with DbSets and Fluent API config in `OnModelCreating`
- `Program.cs` registers `AddDbContext<RecipeCatalogDbContext>(o => o.UseSqlite(...))` and creates a `notifications` directory at startup.
- `RecipesController` injects `RecipeCatalogDbContext` directly and has:
  - `Index(Guid? categoryId, int? difficulty)` — EF Core query with `.Include(r => r.Category)`, optional filtering, maps to `RecipeIndexViewModel` inline
  - `Create` (GET + POST) — GET populates `RecipeFormViewModel` with category dropdown; POST does inline validation (difficulty 1-5, prep time > 0, category exists), saves, writes notification via `File.AppendAllText()`, redirects to Index
  - `Edit` (GET + POST) — GET loads recipe into `RecipeFormViewModel`; POST has **same validation logic duplicated** from Create (**DRY violation**), saves, redirects
  - `Delete` (GET + POST) — GET shows confirmation via `RecipeDeleteViewModel`; POST deletes and redirects
- **ViewModels** (`ViewModels/` folder) are presentation models used by the Views. They stay in WebUi and are **unchanged during the lab** — the refactoring only affects what happens behind the controller.
- **Views** (`Views/Recipes/`) provide a working UI: recipe list with category and difficulty filter dropdowns, create/edit forms with validation messages, and delete confirmation. Bootstrap styling, same approach as the OrderForm lab. **Unchanged during the lab.**
- No interfaces, no service layer, no separation — all logic is in the controller

**Entities:**
- **Recipe:** Id (Guid), Title (string), Description (string?), CategoryId (Guid), Category (navigation), DifficultyLevel (int, 1-5), PrepTimeInMinutes (int, >0)
- **Category:** Id (Guid), Name (string)

**Seed data (4 categories, 8 recipes) — see `seed.sql` below.**

**Intentional problems to identify (supports T01):**
1. Controllers have multiple responsibilities (HTTP, validation, data access, file I/O, ViewModel mapping)
2. EF Core `DbContext` injected directly into controllers — tightly coupled to SQLite
3. `File.AppendAllText` directly in controller — tightly coupled to filesystem
4. Validation logic duplicated in Create (POST) and Edit (POST) — same checks copy-pasted (DRY violation)
5. Business validation inline in controller actions — the entity can't protect its own invariants (DifficultyLevel and PrepTimeInMinutes can be set to invalid values independently)
6. Everything in one project — no architectural boundaries

### 2. "After" State (solution branch)

Clean architecture with 4 projects:

```
Pluralsight.CleanArchitecture.slnx (4 projects)

Pluralsight.CleanArchitecture.Domain/              # Zero dependencies
    Entities/
        Recipe.cs                                   # Rich entity: private setters + UpdatePreparation method
        Category.cs                                 # Clean POCO

Pluralsight.CleanArchitecture.Application/         # Depends on Domain only
    Contracts/
        IRecipeRepository.cs
        ICategoryRepository.cs
        INotificationService.cs                     # Shows infra isn't just databases
    Services/
        IRecipeService.cs                           # Application service interface
        RecipeService.cs                            # Orchestrates repositories + notification
        ICategoryService.cs
        CategoryService.cs
    ApplicationServiceRegistration.cs

Pluralsight.CleanArchitecture.Infrastructure/      # Depends on Application
    Persistence/
        RecipeCatalogDbContext.cs                    # Moved from WebUi, HasData() seed
        RecipeRepository.cs                         # Uses DbContext, implements IRecipeRepository
        CategoryRepository.cs                       # Uses DbContext, implements ICategoryRepository
    Notifications/
        FileNotificationService.cs                  # Writes to file, implements INotificationService
    InfrastructureServiceRegistration.cs

Pluralsight.CleanArchitecture.WebUi/               # Depends on Application + Infrastructure
    Controllers/
        RecipesController.cs                        # Thin: injects IRecipeService + ICategoryService
    ViewModels/                                     # UNCHANGED from before state
        RecipeIndexViewModel.cs
        RecipeFormViewModel.cs
        RecipeDeleteViewModel.cs
    Views/                                          # UNCHANGED from before state
        Recipes/ (Index, Create, Edit, Delete)
        Shared/ (_Layout, _ValidationScriptsPartial)
    Program.cs                                      # Calls AddApplicationServices + AddInfrastructureServices
```

**Dependency direction (inward only):**
- Domain: no project references
- Application: references Domain
- Infrastructure: references Application (and transitively Domain)
- WebUi: references Application + Infrastructure (for DI registration at composition root)

---

## SOLID Principles Coverage

| Principle | Where | How |
|-----------|-------|-----|
| **SRP** | Step 5 | Fat controllers become thin; each service class has one job; each project has one responsibility |
| **OCP** | Step 3 (callout) | New features = new service methods or new service classes. Adding a GetRecipeDetail feature means extending `IRecipeService`, not changing existing working methods. The layered structure means new infrastructure concerns (e.g. caching) can be added without modifying Application or Domain. |
| **LSP** | Step 6 (callout) | Any `IRecipeRepository` implementation can substitute another — that's how we could swap SQLite for SQL Server |
| **ISP** | Step 3 | Three focused contracts (`IRecipeRepository`, `ICategoryRepository`, `INotificationService`) rather than one mega-interface. Two focused services (`IRecipeService`, `ICategoryService`). Controllers and services inject only what they need. |
| **DIP** | Steps 3-5 | Interfaces defined in Application, implemented in Infrastructure. Application never references Infrastructure. Controllers depend on service interfaces, not concrete classes. |

## DRY Demonstration

- **Before:** `RecipesController.Create` (POST) and `RecipesController.Edit` (POST) both contain identical validation logic — checking difficulty is 1-5, prep time > 0, and category exists. The same block of code is copy-pasted between the two actions.
- **After:** The validation lives in one place only. The difficulty/prep time check is in `Recipe.UpdatePreparation()` (entity protects its own invariants). The category-exists check is in `RecipeService.CreateRecipeAsync` / `UpdateRecipeAsync`. No duplication.
- **Called out** in Step 1 (identify the violation) and celebrated in Steps 2-3 (eliminated by domain method + service layer).

---

## Lab Structure: 6 Steps, ~19 Tasks

### Step 1: Introduction and Setup (0 graded tasks — orientation + analysis)

**Covers T01.** Learner runs the app, uses the UI, reads the code, and identifies architectural problems.

Content:
- Lab scenario: "You've inherited a working Recipe Catalog application. It has a UI for browsing, filtering, creating, editing, and deleting recipes. It works — but the code behind it is a mess. Your job is to refactor it into clean architecture."
- Run with `dotnet run`, open the browser, see the recipe list, try the filters, create a recipe, edit one, delete one
- Explore the code and identify problems:
  - Open `RecipesController.cs` — how many responsibilities does this single class have? (HTTP, ViewModel population, validation, EF Core queries, file I/O)
  - Compare the Create (POST) and Edit (POST) actions — notice the duplicated validation block
  - Open `Data/RecipeCatalogDbContext.cs` — why is this in the web project?
  - Open `Models/Recipe.cs` — data annotations tie the entity to a framework
- Brief intro to clean architecture: concentric circles, dependency rule, the goal of this refactoring
- Note: the ViewModels and Views folders will not be touched during this lab. The refactoring happens behind the controller — the UI stays the same.

### Step 2: Create the Domain Layer (4 tasks)

**Objective:** Extract domain entities into a dedicated project with zero dependencies. Entities protect their own invariants.

| Task | What the learner does | Files |
|------|----------------------|-------|
| 2.1 | Run `dotnet new classlib` to create Domain project; add to .slnx | Domain.csproj, .slnx |
| 2.2 | Create `Entities/Category.cs` — clean POCO (Guid Id, string Name) | Domain/Entities/Category.cs |
| 2.3 | Create `Entities/Recipe.cs` with public auto-properties for Id, Title, Description, CategoryId. But for DifficultyLevel and PrepTimeInMinutes, use **private setters**. No data annotations. | Domain/Entities/Recipe.cs |
| 2.4 | Add an `UpdatePreparation(int difficultyLevel, int prepTimeInMinutes)` method to Recipe that validates both values (difficulty 1-5, prep time > 0) and sets both properties together. This ensures the entity can never be put into an invalid state — you can't set difficulty to 6 or prep time to -1, even by accident. | Domain/Entities/Recipe.cs |

**Teaches:**
- The innermost circle has zero dependencies. Entities are plain C# — no `[Required]`, no `[StringLength]`, no EF Core attributes. Persistence ignorance.
- **Rich domain model:** The entity protects its own invariants. In the "before" state, this validation lived in the controller — any new code path that creates or updates a recipe would need to duplicate it. With `UpdatePreparation()`, the rule is enforced in one place, by the entity itself. You can't bypass it because the setters are private.
- This is a taste of **Domain-Driven Design (DDD)** — entities that encapsulate business rules rather than being passive data bags.

### Step 3: Create the Application Layer (6 tasks)

**Objective:** Define contracts (interfaces) and application service classes that orchestrate business logic.

| Task | What the learner does | Files |
|------|----------------------|-------|
| 3.1 | Create Application project, add reference to Domain; add to .slnx | Application.csproj, .slnx |
| 3.2 | Create repository contracts: `Contracts/IRecipeRepository.cs` (GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync) and `Contracts/ICategoryRepository.cs` (GetAllAsync, GetByIdAsync) | Contracts/IRecipeRepository.cs, ICategoryRepository.cs |
| 3.3 | Create `Contracts/INotificationService.cs` with a single method: `SendNotificationAsync(string message)` | Contracts/INotificationService.cs |
| 3.4 | Create `Services/IRecipeService.cs` and `Services/RecipeService.cs` — RecipeService injects IRecipeRepository, ICategoryRepository, INotificationService. Methods: `GetAllAsync` (optional category/difficulty filter, returns list of domain entities), `GetByIdAsync`, `CreateAsync` (validates category exists, creates Recipe, calls `recipe.UpdatePreparation()`, persists, sends notification), `UpdateAsync` (loads entity, validates category, calls `recipe.UpdatePreparation()`, persists), `DeleteAsync` | Services/IRecipeService.cs, RecipeService.cs |
| 3.5 | Create `Services/ICategoryService.cs` and `Services/CategoryService.cs` — CategoryService injects ICategoryRepository. Methods: `GetAllAsync` (returns list of domain entities) | Services/ICategoryService.cs, CategoryService.cs |
| 3.6 | Create `ApplicationServiceRegistration.cs` — extension method on IServiceCollection that registers RecipeService and CategoryService as scoped services against their interfaces | ApplicationServiceRegistration.cs |

**Teaches:**
- **DIP:** Interfaces defined here, implemented elsewhere. Application has no idea SQLite or files exist. Controllers will depend on `IRecipeService`, not the concrete class.
- **ISP:** Three focused infrastructure contracts (`IRecipeRepository`, `ICategoryRepository`, `INotificationService`) rather than one mega-interface. Two focused service interfaces. Each class injects only what it needs.
- **OCP:** Adding a new feature means adding a method to the service — existing working methods stay untouched. New infrastructure concerns can be added without modifying Application or Domain.
- **SRP:** Each service class has a single responsibility area. Validation and orchestration in one place per feature, not scattered across controller actions.
- **DRY:** Validation logic (category exists, difficulty/prep time) lives in one place — the service and domain entity. No more copy-paste between Create and Edit.

### Step 4: Create the Infrastructure Layer (4 tasks)

**Objective:** Implement the contracts with concrete data access and services.

| Task | What the learner does | Files |
|------|----------------------|-------|
| 4.1 | Create Infrastructure project, add `Microsoft.EntityFrameworkCore.Sqlite` package, add reference to Application; add to .slnx | Infrastructure.csproj, .slnx |
| 4.2 | Move `RecipeCatalogDbContext` from WebUi/Data/ to Infrastructure/Persistence/. Update namespace. Update entity references to use Domain entities. Configure with Fluent API (no data annotations on entities). | Infrastructure/Persistence/RecipeCatalogDbContext.cs |
| 4.3 | Create `RecipeRepository` and `CategoryRepository` in Persistence/ — inject DbContext, implement the interface methods using EF Core | Infrastructure/Persistence/RecipeRepository.cs, CategoryRepository.cs |
| 4.4 | Create `FileNotificationService` in Notifications/ and `InfrastructureServiceRegistration.cs` — registers DbContext, repositories (scoped), and notification service (singleton) | Infrastructure/Notifications/FileNotificationService.cs, InfrastructureServiceRegistration.cs |

**Teaches:**
- Infrastructure implements Application's contracts — **the dependency points inward**.
- The database is a detail. Swapping SQLite for SQL Server means changing only this project.
- Infrastructure isn't just databases: `FileNotificationService` shows other I/O concerns belong here too.
- The DbContext uses Fluent API configuration (`OnModelCreating`) to map the clean Domain entities to the existing database schema — no data annotations needed on the entities.

### Step 5: Refactor the Web Layer (4 tasks)

**Objective:** Transform the fat controller into a thin controller that delegates to application services. The Views and ViewModels are unchanged — the UI works exactly as before.

| Task | What the learner does | Files |
|------|----------------------|-------|
| 5.1 | Update WebUi.csproj: add project references to Application + Infrastructure; remove the `Microsoft.EntityFrameworkCore.Sqlite` package (it now lives in Infrastructure) | WebUi.csproj |
| 5.2 | Update Program.cs: replace the DbContext registration with calls to `AddApplicationServices()` and `AddInfrastructureServices()`. Keep `Directory.CreateDirectory` for notifications. | Program.cs |
| 5.3 | Replace `RecipesController` with a thin version: inject `IRecipeService` and `ICategoryService`. Each action calls the service, populates the same ViewModels as before, and returns the same View. All validation, data access, and notification logic is gone — the controller just maps between HTTP/ViewModels and service calls. | Controllers/RecipesController.cs |
| 5.4 | Delete old `Data/RecipeCatalogDbContext.cs` and `Models/Recipe.cs`, `Models/Category.cs` from WebUi. Run `dotnet build` to verify everything compiles. | Delete old files |

**Teaches:**
- **SRP:** The controller's only job is translating HTTP requests into service calls and populating ViewModels. All validation, data access, and notification logic has moved out.
- The web layer is a detail — a delivery mechanism. The same Views and ViewModels work with completely different backend architecture.
- **DI wiring** at the composition root (Program.cs). The only place that knows about all the concrete types.
- The "humble object" pattern: controllers are so thin they barely need testing.

### Step 6: Verify and Sum Up (1 verification task + recap)

**Covers T04.**

| Task | What the learner does |
|------|----------------------|
| 6.1 | Run the app. Verify via the UI: the recipe list displays the seeded recipes; filter by category and difficulty; create a new recipe; edit an existing recipe; delete a recipe. Check that `notifications/recipe-notifications.txt` was created after adding a recipe. The UI behaves identically to Step 1 — but the code behind it is now clean architecture. |

Recap content:
- Started with 1 project, ended with 4 projects
- Every dependency points inward
- Principles demonstrated: SRP, OCP, ISP, DIP, DRY

**Testability — a key benefit of what you've built:**

One of the most important practical benefits of this architecture is testability. Consider what it would take to unit test the "before" `RecipesController` — you'd need a real SQLite database, a real filesystem for notifications, and an HTTP context. Now consider testing `RecipeService`: you can mock `IRecipeRepository`, `ICategoryRepository`, and `INotificationService` with any mocking framework, and test the business logic in complete isolation — no database, no filesystem, no HTTP server. The same applies to the domain: `Recipe.UpdatePreparation()` can be tested with a simple unit test that verifies invalid values throw exceptions. Clean architecture makes testing easy by design, not by accident.

**What would change to swap SQLite for SQL Server?** Only the `Infrastructure` project and a connection string. The Application and Domain layers wouldn't change at all — they don't know SQLite exists.

**Next steps — what clean architecture enables beyond this lab:**
- **Domain-Driven Design (DDD):** You got a taste of this with `Recipe.UpdatePreparation()` — an entity that protects its own invariants. DDD goes much further: aggregates, value objects, domain events (e.g. a `RecipeCreatedEvent` that triggers the notification instead of the service calling it directly), and richer behaviour on entities. This is a natural next step for systems with complex business rules.
- **MediatR and the Mediator pattern:** Instead of controllers calling service classes directly, you can use MediatR to dispatch requests to handlers. Each handler is a self-contained class for a single operation (e.g. `GetRecipeListQueryHandler`). This enforces CQRS (Command Query Responsibility Segregation) structurally — queries and commands become distinct types. It also makes OCP even stronger: adding a new feature means adding a new handler class, never modifying existing ones.
- **FluentValidation:** Move validation rules into dedicated validator classes, automatically invoked before your service logic runs.
- **AutoMapper:** Replace manual entity-to-DTO mapping with convention-based profiles.
- **Cross-cutting concerns:** Custom middleware for global error handling (converting exceptions to consistent HTTP responses), Serilog for structured logging, pipeline behaviors for validation/caching applied across all operations without modifying individual services.

---

## Implementation Order

### Phase A: Build the "before" state on `main`
1. Initialize git repo on `main` branch
2. Add `Microsoft.EntityFrameworkCore.Sqlite` package
3. Create `Models/Recipe.cs` and `Models/Category.cs` with data annotations
4. Create `Data/RecipeCatalogDbContext.cs` with DbSets + Fluent API config
5. Create `seed.sql` and generate the pre-seeded `recipecatalog.db` file
6. Create `ViewModels/` — RecipeIndexViewModel, RecipeFormViewModel, RecipeDeleteViewModel
7. Create `Views/Recipes/` — Index, Create, Edit, Delete (Bootstrap forms, filter dropdowns)
8. Update `Views/Shared/_Layout.cshtml` — Recipe Catalog branding, simplified nav
9. Create fat `RecipesController.cs` (EF Core queries, File.AppendAllText, duplicated validation, all CRUD)
10. Update `Program.cs`: register DbContext with SQLite, Directory.CreateDirectory, default route to Recipes
11. Add `notifications/` to .gitignore (the .db file IS committed — it ships with the lab)
12. Verify: `dotnet run`, browse the UI, test all CRUD operations + filtering
13. Commit on `main`

### Phase B: Build the "after" state on `solution`
1. Create `solution` branch from `main`
2. Create Domain project + rich entities (Recipe with UpdatePreparation, Category POCO)
3. Create Application project + contracts + service classes + service registration
4. Create Infrastructure project: move DbContext, create repositories + FileNotificationService + service registration
5. Refactor WebUi: update references, update Program.cs, replace controller, delete old Models/ and Data/
6. Verify: `dotnet run`, browse the UI — same behaviour as before, confirm notifications file
7. Commit on `solution`

### Phase C: Write lab content
1. Create `docs/lab-outline.md` (concepts, audience, abstract, description)
2. Create `docs/lab-content.md` (full step/task content with Semgrep checks)
3. Follow exact format from OrderForm reference lab

---

## Key Files to Modify/Create

**Before state (main branch):**
- `Pluralsight.CleanArchitecture.WebUi/Models/Recipe.cs` — new (entity with data annotations)
- `Pluralsight.CleanArchitecture.WebUi/Models/Category.cs` — new (entity with data annotations)
- `Pluralsight.CleanArchitecture.WebUi/Data/RecipeCatalogDbContext.cs` — new
- `Pluralsight.CleanArchitecture.WebUi/Controllers/RecipesController.cs` — new (fat, all CRUD)
- `Pluralsight.CleanArchitecture.WebUi/ViewModels/RecipeIndexViewModel.cs` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/ViewModels/RecipeFormViewModel.cs` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/ViewModels/RecipeDeleteViewModel.cs` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/Views/Recipes/Index.cshtml` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/Views/Recipes/Create.cshtml` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/Views/Recipes/Edit.cshtml` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/Views/Recipes/Delete.cshtml` — new (unchanged during lab)
- `Pluralsight.CleanArchitecture.WebUi/Views/Shared/_Layout.cshtml` — modified (Recipe Catalog branding)
- `Pluralsight.CleanArchitecture.WebUi/Program.cs` — modify (DbContext, default route)
- `Pluralsight.CleanArchitecture.WebUi/Pluralsight.CleanArchitecture.WebUi.csproj` — modify (add EF Core package)
- `Pluralsight.CleanArchitecture.WebUi/recipecatalog.db` — new (pre-seeded, committed)
- `seed.sql` — new (SQL seed script for reference/reproducibility)
- `.gitignore` — new (ignores notifications/ only)

**After state (solution branch) — changes from main:**
- `Pluralsight.CleanArchitecture.Domain/` — new project (~3 files)
- `Pluralsight.CleanArchitecture.Application/` — new project (~8 files: 3 contracts, 2 service interfaces, 2 service implementations, 1 registration)
- `Pluralsight.CleanArchitecture.Infrastructure/` — new project (~6 files)
- `Pluralsight.CleanArchitecture.slnx` — modified (4 projects)
- `Pluralsight.CleanArchitecture.WebUi/Controllers/RecipesController.cs` — replaced (thin)
- `Pluralsight.CleanArchitecture.WebUi/Program.cs` — modified (DI calls)
- `Pluralsight.CleanArchitecture.WebUi/Pluralsight.CleanArchitecture.WebUi.csproj` — modified (references, remove EF package)
- `Pluralsight.CleanArchitecture.WebUi/Data/` — deleted
- `Pluralsight.CleanArchitecture.WebUi/Models/Recipe.cs` — deleted
- `Pluralsight.CleanArchitecture.WebUi/Models/Category.cs` — deleted
- ViewModels/ and Views/ — **UNCHANGED**

**Lab docs:**
- `docs/lab-outline.md` — new
- `docs/lab-content.md` — new

---

## Verification

1. **Before state:** `dotnet build` succeeds; `dotnet run` starts; UI shows recipe list with filtering, CRUD forms all work; `notifications/recipe-notifications.txt` is created when a recipe is added
2. **After state:** `dotnet build` succeeds; `dotnet run` starts; UI behaves identically; notification file still works; all 4 projects compile independently
3. **Lab content:** Semgrep rules match the solution branch code; step progression is logical; each task is completable in 2-5 minutes; T01-T04 objectives all covered

---

## Seed Data (seed.sql)

This script creates the schema and populates the pre-seeded `recipecatalog.db` shipped with the lab. The table names and column names must match what EF Core expects from the DbContext configuration.

```sql
CREATE TABLE IF NOT EXISTS Categories (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Recipes (
    Id TEXT NOT NULL PRIMARY KEY,
    Title TEXT NOT NULL,
    Description TEXT,
    CategoryId TEXT NOT NULL,
    DifficultyLevel INTEGER NOT NULL,
    PrepTimeInMinutes INTEGER NOT NULL,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

-- Categories
INSERT INTO Categories (Id, Name) VALUES ('b1c2d3e4-1111-4000-a000-000000000001', 'Breakfast');
INSERT INTO Categories (Id, Name) VALUES ('b1c2d3e4-2222-4000-a000-000000000002', 'Main Course');
INSERT INTO Categories (Id, Name) VALUES ('b1c2d3e4-3333-4000-a000-000000000003', 'Dessert');
INSERT INTO Categories (Id, Name) VALUES ('b1c2d3e4-4444-4000-a000-000000000004', 'Appetizer');

-- Breakfast recipes
INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0001-4000-a000-000000000001', 'Classic Pancakes',
        'Fluffy buttermilk pancakes served with maple syrup and fresh berries.',
        'b1c2d3e4-1111-4000-a000-000000000001', 1, 20);

INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0002-4000-a000-000000000002', 'Eggs Benedict',
        'Poached eggs on toasted muffins with hollandaise sauce and smoked salmon.',
        'b1c2d3e4-1111-4000-a000-000000000001', 3, 35);

-- Main Course recipes
INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0003-4000-a000-000000000003', 'Grilled Chicken',
        'Herb-marinated chicken breast grilled and served with roasted vegetables.',
        'b1c2d3e4-2222-4000-a000-000000000002', 2, 45);

INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0004-4000-a000-000000000004', 'Beef Stew',
        'Slow-cooked beef with root vegetables in a rich red wine sauce.',
        'b1c2d3e4-2222-4000-a000-000000000002', 3, 120);

INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0005-4000-a000-000000000005', 'Mushroom Risotto',
        'Creamy arborio rice with mixed wild mushrooms and parmesan.',
        'b1c2d3e4-2222-4000-a000-000000000002', 4, 50);

-- Dessert recipes
INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0006-4000-a000-000000000006', 'Chocolate Lava Cake',
        'Individual warm chocolate cakes with a molten centre, served with vanilla ice cream.',
        'b1c2d3e4-3333-4000-a000-000000000003', 4, 30);

INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0007-4000-a000-000000000007', 'Lemon Tart',
        'Crisp shortcrust pastry filled with tangy lemon curd and topped with meringue.',
        'b1c2d3e4-3333-4000-a000-000000000003', 3, 60);

-- Appetizer recipe
INSERT INTO Recipes (Id, Title, Description, CategoryId, DifficultyLevel, PrepTimeInMinutes)
VALUES ('a0000001-0008-4000-a000-000000000008', 'Bruschetta',
        'Toasted ciabatta topped with fresh tomatoes, basil, garlic, and olive oil.',
        'b1c2d3e4-4444-4000-a000-000000000004', 1, 15);
```

The GUIDs use a predictable pattern for readability during development. The data provides:
- All 4 categories are used
- Good spread of difficulty levels (1-4) and prep times (15-120 min)
- Enough variety for meaningful GET responses
- At least one category with multiple recipes (Main Course has 3) to test the "can't delete category with recipes" business rule
