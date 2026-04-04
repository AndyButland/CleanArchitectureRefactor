# Guided: Refactoring to Clean Architecture with ASP.NET Core 10

## Step 1: Introduction and Project Exploration

### Introduction to Clean Architecture

There's a reason that many developers like working on new, greenfield projects. A new codebase is small, easy to navigate, and quick to change. But as features accumulate, the effort required to make changes grows steadily, and progress slows down. New functionality takes longer to add. Bug fixes in one area introduce regressions in another. Developers are spending more time untangling code than writing it whilst stakeholders look on and wonder why "just this little update" is taking so long to deliver.

This is what can happen when architecture decisions are left to drift. Software should be *soft* — meaning easy to modify. The goal is to keep it that way: to minimize the human effort required to build and maintain a system, not just at the beginning but over its lifetime.

**Clean architecture** is one proven approach to achieving this. It organizes code into concentric layers, with business rules at the center and infrastructure concerns like databases, frameworks, and file systems at the edges. The golden rule is simple: dependencies point only inward. This means you can change how data is stored, how notifications are sent, or how the UI works — without touching business logic.

These architectural boundaries work hand-in-hand with good design at the class and method level. The **SOLID principles** guide how individual components are shaped within each layer.

In this lab, you will apply these ideas with hands-on refactoring. You will take a working but poorly structured application and modify it into four clearly separated layers, each with well-defined boundaries.

### Lab Scenario

You have been brought in to improve the architecture of a Recipe Catalog application built with ASP.NET 10. The application works — users can browse, filter, create, edit, and delete recipes through a web interface — but all of the logic lives in a single project with "fat controllers" that directly access the database, write to the filesystem, and duplicate validation code.

Your job is to refactor this application into a clean architecture without changing its external behavior. The user interface will continue to work identically throughout.

In the project folder `Pluralsight.CleanArchitecture.Web`, there is an ASP.NET MVC application that you will work on to restructure into separate domain, application, infrastructure, and presentation layers.

### Working with the Project

Run the following command in the terminal to access the application folder and run it:

```
cd Pluralsight.CleanArchitecture.Web
dotnet run
```

Click on the **Web Browser** tab and then on the **Open in new browser tab** button. You will then see the web application running.

> Whenever you make changes to the code while working on the tasks, you need to stop and re-run the app so your changes take effect. You can do that by pressing `CTRL+C` in the terminal then running `dotnet run` again. If you don't need to see changes, you can also run `dotnet build`, just as a check that the code continues to compile.

Use the application to browse the recipe list, try the category and difficulty filters, create a new recipe, edit an existing one, and delete one. Everything works just fine. It's under the covers that things can be improved.

### Exploring the Code

Explore the files in the `Pluralsight.CleanArchitecture.Web` folder to understand how the application is currently structured.

* In the `Controllers` folder, open `RecipesController.cs`. This single controller handles all CRUD operations for recipes. Notice how many responsibilities it has: HTTP request handling, Entity Framework Core data access, business validation, file writing for notifications, and mapping to view models. All in one class, clearly violating the **Single Responsibility Principle** (the "S" in SOLID). Compare also the `Create` (POST) and `Edit` (POST) actions and notice the duplicated validation logic.
* In the `Data` folder, you will find `RecipeCatalogDbContext.cs`. This is the Entity Framework Core database context. Currently it lives directly in the Web project. We have tight coupling of the presentation layer to the data access technology.
* In the `Models` folder, you will find `Recipe.cs` and `Category.cs`. These entity classes can be considered **anaemic**. They are passive holders of data, with all properties freely settable, and little behavior to protect business rules. The validation logic that should belong to the entity (like difficulty being between 1 and 5) lives in the controller instead.
* In the `ViewModels` folder, there are view models used by the Razor views.
* In the `Views/Recipes` folder, you will find the Razor views for the recipe list, create, edit, and delete pages.

There are other files necessary for bootstrapping the web application and rendering the pages that you can review. None are essential to understand for the purposes of the lab nor will you be modifying them.

## Step 2: Create the Domain Layer

In clean architecture, the Domain layer sits at the center. It has no dependencies on any other project, framework, or library, and no direct interaction with databases, files or network services. This is where your core business logic is held, with real-world business objects and relations modelled as **entities**.

In the current codebase, `Recipe` and `Category` are simple classes with public getters and setters that any code can modify freely. There is nothing stopping a caller from setting a recipe's difficulty level to 99 or its preparation time to a negative number. Currently those checks happen in the controller, but as the application grows, and other screens, channels and background services start to work with recipes, it's going to be easy to miss applying this validation. And if changes are needed to the business rules, easy to miss making updates too. Much better to centralize this into one place.

In this step, you will create a dedicated Domain project and build entity classes that take ownership of their own rules. The `Recipe` entity will use private setters and a dedicated method to ensure its properties can never be set to invalid values, no matter where in the application it is used.

### Task 2.1: Create the Domain project

If you are still in the `Pluralsight.CleanArchitecture.Web` folder from running the app earlier, navigate up to the root solution folder first:

```
cd ..
```

Then run the following commands to create a new class library project and add it to the solution:

```
dotnet new classlib -n Pluralsight.CleanArchitecture.Domain -o Pluralsight.CleanArchitecture.Domain
dotnet sln Pluralsight.CleanArchitecture.slnx add Pluralsight.CleanArchitecture.Domain/Pluralsight.CleanArchitecture.Domain.csproj
```

The first command creates a class library project — not a web project — because the Domain layer has no need for ASP.NET. The second adds it to the solution file.

Delete the auto-generated `Class1.cs` file from the new project, and create an `Entities` folder inside `Pluralsight.CleanArchitecture.Domain`. This is where your entity classes will live.

---
Check: Pluralsight.CleanArchitecture.Domain/Pluralsight.CleanArchitecture.Domain.csproj

```yaml
rules:
- id: task_2_1_domain_csproj
  pattern-regex: <Project\s+Sdk\s*=\s*"Microsoft\.NET\.Sdk">
  message: Create the Domain class library project using `dotnet new classlib`.
  languages: [generic]
  severity: WARNING
```

---

### Task 2.2: Create the Category entity

Create a new file `Category.cs` inside the `Entities` folder of the Domain project.

Define a `Category` class in the `Pluralsight.CleanArchitecture.Domain.Entities` namespace with two public properties:

- `Id` as `Guid`
- `Name` as `string`, initialized to `string.Empty`

---
Check: Pluralsight.CleanArchitecture.Domain/Entities/Category.cs

```yaml
rules:
- id: task_2_2_category_id
  pattern-regex: public\s+Guid\s+Id\s*\{\s*get\s*;\s*set\s*;\s*\}
  message: Add a public `Id` property of type `Guid` to the `Category` class.
  languages: [C#]
  severity: WARNING

- id: task_2_2_category_name
  pattern-regex: public\s+string\s+Name\s*\{\s*get\s*;\s*set\s*;\s*\}\s*=\s*(string\.Empty|"");
  message: Add a public `Name` property of type `string`, initialized to `string.Empty`.
  languages: [C#]
  severity: WARNING
```

---

Notice that unlike the existing `Category` model in the Web project, this class has no data annotation attributes like `[Required]` or `[StringLength]`. The domain entity is a plain C# class. Database constraints and validation rules will be handled elsewhere: by the Infrastructure layer and the entity's own methods, respectively.

### Task 2.3: Create the Recipe entity

Create a new file `Recipe.cs` inside the `Entities` folder of the Domain project.

Define a `Recipe` class in the `Pluralsight.CleanArchitecture.Domain.Entities` namespace with the following public properties:

- `Id` as `Guid`
- `Title` as `string`, initialized to `string.Empty`
- `Description` as `string?` (nullable)
- `CategoryId` as `Guid`
- `Category` as `Category` (navigation property)
- `DifficultyLevel` as `int`
- `PrepTimeInMinutes` as `int`

---
Check: Pluralsight.CleanArchitecture.Domain/Entities/Recipe.cs

```yaml
rules:
- id: task_2_3_recipe_id
  pattern-regex: public\s+Guid\s+Id\s*\{\s*get\s*;\s*set\s*;\s*\}
  message: Add a public `Id` property of type `Guid` to the `Recipe` class.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_title
  pattern-regex: public\s+string\s+Title\s*\{\s*get\s*;\s*set\s*;\s*\}\s*=\s*(string\.Empty|"");
  message: Add a public `Title` property of type `string`, initialized to `string.Empty`.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_description
  pattern-regex: public\s+string\?\s+Description\s*\{\s*get\s*;\s*set\s*;\s*\}
  message: Add a public `Description` property of type `string?`.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_categoryid
  pattern-regex: public\s+Guid\s+CategoryId\s*\{\s*get\s*;\s*set\s*;\s*\}
  message: Add a public `CategoryId` property of type `Guid`.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_category
  pattern-regex: public\s+Category\s+Category\s*\{\s*get\s*;\s*set\s*;\s*\}
  message: Add a public `Category` navigation property of type `Category`.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_difficulty
  pattern-regex: public\s+int\s+DifficultyLevel\s*\{\s*get\s*;
  message: Add a public `DifficultyLevel` property of type `int`.
  languages: [C#]
  severity: WARNING

- id: task_2_3_recipe_preptime
  pattern-regex: public\s+int\s+PrepTimeInMinutes\s*\{\s*get\s*;
  message: Add a public `PrepTimeInMinutes` property of type `int`.
  languages: [C#]
  severity: WARNING
```

---

This gives you the same data shape as the existing `Recipe` model, but without any framework attributes. In the next task you will make an important change to how two of these properties are set.

### Task 2.4: Add private setters and the UpdatePreparation method

In the `Recipe` class, change the setters for `DifficultyLevel` and `PrepTimeInMinutes` from `set` to `private set`. This prevents any code outside the class from setting these properties directly.

Then add a public method called `UpdatePreparation` that accepts two parameters: `int difficultyLevel` and `int prepTimeInMinutes`. Inside this method:

1. Validate that `difficultyLevel` is between 1 and 5 (inclusive). If not, throw an `ArgumentOutOfRangeException` with the parameter name and the message `"Difficulty must be between 1 and 5."`.
2. Validate that `prepTimeInMinutes` is greater than zero. If not, throw an `ArgumentOutOfRangeException` with the parameter name and the message `"Prep time must be greater than zero."`.
3. Validate that if `prepTimeInMinutes` is greater than 60, then `difficultyLevel` must be at least 3. If not, throw an `ArgumentException` with the message `"Recipes over one hour must have a difficulty of at least 3."`.
4. If all validations pass, assign both values to the `DifficultyLevel` and `PrepTimeInMinutes` properties.

---
Check: Pluralsight.CleanArchitecture.Domain/Entities/Recipe.cs

```yaml
rules:
- id: task_2_4_difficulty_private_set
  pattern-regex: public\s+int\s+DifficultyLevel\s*\{\s*get\s*;\s*private\s+set\s*;\s*\}
  message: Change `DifficultyLevel` to use `private set`.
  languages: [C#]
  severity: WARNING

- id: task_2_4_preptime_private_set
  pattern-regex: public\s+int\s+PrepTimeInMinutes\s*\{\s*get\s*;\s*private\s+set\s*;\s*\}
  message: Change `PrepTimeInMinutes` to use `private set`.
  languages: [C#]
  severity: WARNING

- id: task_2_4_update_preparation_method
  pattern-regex: public\s+void\s+UpdatePreparation\s*\(\s*int\s+difficultyLevel\s*,\s*int\s+prepTimeInMinutes\s*\)
  message: Add a public `UpdatePreparation` method that accepts `int difficultyLevel` and `int prepTimeInMinutes`.
  languages: [C#]
  severity: WARNING

- id: task_2_4_difficulty_validation
  pattern-regex: difficultyLevel\s*<\s*1\s*\|\|\s*difficultyLevel\s*>\s*5
  message: Validate that `difficultyLevel` is between 1 and 5.
  languages: [C#]
  severity: WARNING

- id: task_2_4_preptime_validation
  pattern-regex: prepTimeInMinutes\s*<=\s*0
  message: Validate that `prepTimeInMinutes` is greater than zero.
  languages: [C#]
  severity: WARNING

- id: task_2_4_cross_validation
  pattern-regex: prepTimeInMinutes\s*>\s*60\s*&&\s*difficultyLevel\s*<\s*3
  message: Validate that recipes over one hour must have a difficulty of at least 3.
  languages: [C#]
  severity: WARNING
```

---

This is a meaningful shift from the "before" state. Previously, this validation logic was duplicated across the `Create` and `Edit` actions in the controller. Now it lives in one place, the entity itself, and cannot be bypassed. The entity protects its own invariants, which is a core idea in **domain-driven design**. Any code that needs to set these values must go through `UpdatePreparation`, whether it's called from a service, a test, or anywhere else.

## Step 3: Create the Application Layer

The Application layer sits between the domain and the outside world. It defines *what the application can do*, through service interfaces and their implementations, without knowing *how* things like databases or file systems work. It depends on the Domain project, but nothing else.

This is where the **Dependency Inversion Principle** (the "D" in SOLID) comes in. Instead of the Application layer calling a database directly, it defines interfaces (contracts) that describe what it needs. Examples in our application are "give me all recipes" or "send a notification." The actual implementation of those contracts will live in the Infrastructure layer, which you will build in the next step.

This separation is what makes clean architecture flexible. If you later need to swap database technology, or replace file-based notifications with an email service, you change only the Infrastructure layer. The Application layer, and all of its business logic, remains untouched.

### Task 3.1: Create the Application project with a reference to Domain

Run the following commands to create the Application project, add it to the solution, and add a project reference to Domain:

```
dotnet new classlib -n Pluralsight.CleanArchitecture.Application -o Pluralsight.CleanArchitecture.Application
dotnet sln Pluralsight.CleanArchitecture.slnx add Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj
dotnet add Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj reference Pluralsight.CleanArchitecture.Domain/Pluralsight.CleanArchitecture.Domain.csproj
```

Delete the auto-generated `Class1.cs` file from the new project.

Then create two folders inside `Pluralsight.CleanArchitecture.Application`:

- `Contracts` — for the interfaces that define what the application needs from external systems.
- `Services` — for the application service interfaces and their implementations.

---
Check: Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj

```yaml
rules:
- id: task_3_1_application_csproj
  pattern-regex: <ProjectReference\s+Include=.*CleanArchitecture\.Domain.*\.csproj
  message: Add a project reference from the Application project to the Domain project.
  languages: [generic]
  severity: WARNING
```

---

The project reference to Domain means the Application layer can use the `Recipe` and `Category` entities. But notice the direction: Application depends on Domain, not the other way around. Domain remains completely independent.

### Task 3.2: Define the repository contracts

Repository contracts define how the Application layer expects to interact with data storage, without specifying the technology behind it.

Create a new file `IRecipeRepository.cs` inside the `Contracts` folder. Define an interface in the `Pluralsight.CleanArchitecture.Application.Contracts` namespace with the following methods:

- `Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null)` — returns recipes, optionally filtered
- `Task<Recipe?> GetByIdAsync(Guid id)` — returns a single recipe or null
- `Task<Recipe> AddAsync(Recipe recipe)` — adds a recipe and returns it
- `Task UpdateAsync(Recipe recipe)` — updates an existing recipe
- `Task DeleteAsync(Guid id)` — deletes a recipe by ID

You will need a `using` statement for `Pluralsight.CleanArchitecture.Domain.Entities`.

Then create `ICategoryRepository.cs` in the same folder with:

- `Task<List<Category>> GetAllAsync()` — returns all categories
- `Task<Category?> GetByIdAsync(Guid id)` — returns a single category or null

---
Check: Pluralsight.CleanArchitecture.Application/Contracts/IRecipeRepository.cs

```yaml
rules:
- id: task_3_2_ireciperepository_interface
  pattern-regex: public\s+interface\s+IRecipeRepository
  message: Define a public interface named `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_ireciperepository_getall
  pattern-regex: Task<List<Recipe>>\s+GetAllAsync
  message: Add a `GetAllAsync` method returning `Task<List<Recipe>>` to `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_ireciperepository_getbyid
  pattern-regex: Task<Recipe\?>\s+GetByIdAsync
  message: Add a `GetByIdAsync` method returning `Task<Recipe?>` to `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_ireciperepository_add
  pattern-regex: Task<Recipe>\s+AddAsync
  message: Add an `AddAsync` method returning `Task<Recipe>` to `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_ireciperepository_update
  pattern-regex: Task\s+UpdateAsync
  message: Add an `UpdateAsync` method to `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_ireciperepository_delete
  pattern-regex: Task\s+DeleteAsync
  message: Add a `DeleteAsync` method to `IRecipeRepository`.
  languages: [C#]
  severity: WARNING
```

Check: Pluralsight.CleanArchitecture.Application/Contracts/ICategoryRepository.cs

```yaml
rules:
- id: task_3_2_icategoryrepository_interface
  pattern-regex: public\s+interface\s+ICategoryRepository
  message: Define a public interface named `ICategoryRepository`.
  languages: [C#]
  severity: WARNING

- id: task_3_2_icategoryrepository_getall
  pattern-regex: Task<List<Category>>\s+GetAllAsync
  message: Add a `GetAllAsync` method returning `Task<List<Category>>` to `ICategoryRepository`.
  languages: [C#]
  severity: WARNING
```

---

Notice that these interfaces use the domain entities as their data types. The Application layer speaks in terms of the domain. Also see that we have two focused interfaces rather than one large one. Each consumer can depend on only what it needs, which is the **Interface Segregation Principle** (the "I" in SOLID) in action.

### Task 3.3: Define the notification service contract

Not all infrastructure is about databases. The current application writes notifications to a file when a recipe is created. This is also an infrastructure concern that should be abstracted.

Create a new file `INotificationService.cs` inside the `Contracts` folder. Define an interface in the `Pluralsight.CleanArchitecture.Application.Contracts` namespace with a single method:

- `Task SendNotificationAsync(string message)`

---
Check: Pluralsight.CleanArchitecture.Application/Contracts/INotificationService.cs

```yaml
rules:
- id: task_3_3_inotificationservice_interface
  pattern-regex: public\s+interface\s+INotificationService
  message: Define a public interface named `INotificationService`.
  languages: [C#]
  severity: WARNING

- id: task_3_3_sendnotification_method
  pattern-regex: Task\s+SendNotificationAsync\s*\(\s*string\s+message\s*\)
  message: Add a `SendNotificationAsync` method that accepts a `string message` parameter.
  languages: [C#]
  severity: WARNING
```

---

Notice that this is a deliberately simple interface. The Application layer only needs to know that it can send a notification. It does not care whether that notification is written to a file, sent as an email, or posted to a message queue. That decision belongs to the Infrastructure layer.

-

The Application layer isn't only entities and interfaces though. Application services are also created to orchestrate the business logic. They coordinate between repositories, domain entities, and other services to carry out use cases.

### Task 3.4: Create the recipe service interface and implementation

Create a new file `IRecipeService.cs` inside the `Services` folder. You will need a `using` statement for `Pluralsight.CleanArchitecture.Domain.Entities`. Define an interface in the `Pluralsight.CleanArchitecture.Application.Services` namespace with the following methods:

- `Task<List<Recipe>> GetAllAsync(Guid? categoryId = null, int? difficulty = null)`
- `Task<Recipe?> GetByIdAsync(Guid id)`
- `Task<Guid> CreateAsync(string title, string? description, Guid categoryId, int difficultyLevel, int prepTimeInMinutes)`
- `Task UpdateAsync(Guid id, string title, string? description, Guid categoryId, int difficultyLevel, int prepTimeInMinutes)`
- `Task DeleteAsync(Guid id)`

Then create `RecipeService.cs` in the same folder. You will need `using` statements for `Pluralsight.CleanArchitecture.Application.Contracts` and `Pluralsight.CleanArchitecture.Domain.Entities`. This class should implement `IRecipeService` and accept three constructor parameters: `IRecipeRepository`, `ICategoryRepository`, and `INotificationService`. Store them as private readonly fields.

Implement the methods as follows:

- **GetAllAsync**: delegate to `_recipeRepository.GetAllAsync`, passing through the filter parameters.
- **GetByIdAsync**: delegate to `_recipeRepository.GetByIdAsync`.
- **CreateAsync**: look up the category using `_categoryRepository.GetByIdAsync`. If the category is null, throw an `ArgumentException` with the message `"The selected category does not exist."`. Otherwise, create a new `Recipe` with a new `Guid`, the provided `title`, `description`, and `categoryId`. Call `recipe.UpdatePreparation(difficultyLevel, prepTimeInMinutes)` to set and validate the preparation fields. Then call `_recipeRepository.AddAsync` to persist it, and `_notificationService.SendNotificationAsync` with a message like `$"New recipe created: {recipe.Title} (ID: {recipe.Id})"`. Return the recipe's `Id`.
- **UpdateAsync**: load the recipe using `_recipeRepository.GetByIdAsync`. If null, throw an `ArgumentException` with `"Recipe not found."`. Validate the category the same way as in `CreateAsync`. Update the recipe's `Title`, `Description`, and `CategoryId` properties directly, then call `recipe.UpdatePreparation(difficultyLevel, prepTimeInMinutes)`. Finally call `_recipeRepository.UpdateAsync`.
- **DeleteAsync**: delegate to `_recipeRepository.DeleteAsync`.

---
Check: Pluralsight.CleanArchitecture.Application/Services/IRecipeService.cs

```yaml
rules:
- id: task_3_4_irecipeservice_interface
  pattern-regex: public\s+interface\s+IRecipeService
  message: Define a public interface named `IRecipeService`.
  languages: [C#]
  severity: WARNING
```

Check: Pluralsight.CleanArchitecture.Application/Services/RecipeService.cs

```yaml
rules:
- id: task_3_4_recipeservice_class
  pattern-regex: public\s+class\s+RecipeService\s*:\s*IRecipeService
  message: Create a `RecipeService` class that implements `IRecipeService`.
  languages: [C#]
  severity: WARNING

- id: task_3_4_recipeservice_repo_dependency
  pattern-regex: IRecipeRepository\s+\w+
  message: Inject `IRecipeRepository` into `RecipeService`.
  languages: [C#]
  severity: WARNING

- id: task_3_4_recipeservice_category_dependency
  pattern-regex: ICategoryRepository\s+\w+
  message: Inject `ICategoryRepository` into `RecipeService`.
  languages: [C#]
  severity: WARNING

- id: task_3_4_recipeservice_notification_dependency
  pattern-regex: INotificationService\s+\w+
  message: Inject `INotificationService` into `RecipeService`.
  languages: [C#]
  severity: WARNING

- id: task_3_4_recipeservice_update_preparation
  pattern-regex: \.UpdatePreparation\s*\(
  message: Call `UpdatePreparation` on the recipe entity in `CreateAsync` and `UpdateAsync`.
  languages: [C#]
  severity: WARNING

- id: task_3_4_recipeservice_send_notification
  pattern-regex: SendNotificationAsync\s*\(
  message: Call `SendNotificationAsync` when creating a recipe.
  languages: [C#]
  severity: WARNING
```

---

Compare this to the "before" state. The `Create` and `Edit` actions in the controller both contained the same validation checks and data access logic. Now the service handles it once, and both the `CreateAsync` and `UpdateAsync` methods use `recipe.UpdatePreparation()` to enforce the business rules. The duplication is gone.

### Task 3.5: Create the category service interface and implementation

Create `ICategoryService.cs` inside the `Services` folder. You will need a `using` statement for `Pluralsight.CleanArchitecture.Domain.Entities`. Define an interface with a single method:

- `Task<List<Category>> GetAllAsync()`

Then create `CategoryService.cs` in the same folder. You will need `using` statements for `Pluralsight.CleanArchitecture.Application.Contracts` and `Pluralsight.CleanArchitecture.Domain.Entities`. It should implement `ICategoryService`, accept `ICategoryRepository` as a constructor parameter, and delegate `GetAllAsync` to the repository.

---
Check: Pluralsight.CleanArchitecture.Application/Services/ICategoryService.cs

```yaml
rules:
- id: task_3_5_icategoryservice_interface
  pattern-regex: public\s+interface\s+ICategoryService
  message: Define a public interface named `ICategoryService`.
  languages: [C#]
  severity: WARNING
```

Check: Pluralsight.CleanArchitecture.Application/Services/CategoryService.cs

```yaml
rules:
- id: task_3_5_categoryservice_class
  pattern-regex: public\s+class\s+CategoryService\s*:\s*ICategoryService
  message: Create a `CategoryService` class that implements `ICategoryService`.
  languages: [C#]
  severity: WARNING
```

---

The Application layer needs a way to register its services with the dependency injection container. Rather than putting this in the Web project's `Program.cs`, each layer will provide its own registration method. This keeps the Web project from needing to know about the internal classes of each layer.

### Task 3.6: Create the application service registration

Create a new file `ApplicationServiceRegistration.cs` in the root of the Application project.

Define a static class `ApplicationServiceRegistration` in the `Pluralsight.CleanArchitecture.Application` namespace. Add a public static extension method on `IServiceCollection` called `AddApplicationServices` that returns `IServiceCollection`.

Inside the method, register the two services as scoped:

- `IRecipeService` to `RecipeService`
- `ICategoryService` to `CategoryService`

Return the `services` parameter to allow method chaining.

You will need `using` statements for `Microsoft.Extensions.DependencyInjection` and `Pluralsight.CleanArchitecture.Application.Services`. To make the first available, add the `Microsoft.Extensions.DependencyInjection.Abstractions` NuGet package to the Application project:

```
dotnet add Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

---
Check: Pluralsight.CleanArchitecture.Application/ApplicationServiceRegistration.cs

```yaml
rules:
- id: task_3_6_application_registration_class
  pattern-regex: public\s+static\s+class\s+ApplicationServiceRegistration
  message: Create a static class named `ApplicationServiceRegistration`.
  languages: [C#]
  severity: WARNING

- id: task_3_6_add_application_services_method
  pattern-regex: public\s+static\s+IServiceCollection\s+AddApplicationServices\s*\(\s*this\s+IServiceCollection
  message: Add a `AddApplicationServices` extension method on `IServiceCollection`.
  languages: [C#]
  severity: WARNING

- id: task_3_6_register_recipe_service
  pattern-regex: AddScoped<IRecipeService\s*,\s*RecipeService>
  message: Register `IRecipeService` to `RecipeService` as scoped.
  languages: [C#]
  severity: WARNING

- id: task_3_6_register_category_service
  pattern-regex: AddScoped<ICategoryService\s*,\s*CategoryService>
  message: Register `ICategoryService` to `CategoryService` as scoped.
  languages: [C#]
  severity: WARNING
```

---

## Step 4: Create the Infrastructure Layer

The Infrastructure layer is where your application meets the outside world. Databases, file systems, email services, message queues — anything that involves I/O lives here. Dependencies can freely be taken on the specific libraries required to implement the functionality of the application.

Crucially, this layer depends on the Application layer, not the other way around. It *implements* the contracts that Application defined.

In this step you will create the Infrastructure project, move the database context out of the Web project, create repository implementations, implement the notification service, and wire everything together.

### Task 4.1: Create the Infrastructure project

Run the following commands to create the Infrastructure project, add it to the solution, and add a reference to the Application project:

```
dotnet new classlib -n Pluralsight.CleanArchitecture.Infrastructure -o Pluralsight.CleanArchitecture.Infrastructure
dotnet sln Pluralsight.CleanArchitecture.slnx add Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj
dotnet add Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj reference Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj
```

Delete the auto-generated `Class1.cs` file from the new project.

Then add the NuGet packages that the Infrastructure layer needs:

```
dotnet add Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj package Microsoft.EntityFrameworkCore.InMemory
dotnet add Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
```

Finally, create two folders inside the Infrastructure project:

- `Persistence` — for the database context and repository classes.
- `Notifications` — for the notification service implementation.

---
Check: Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj

```yaml
rules:
- id: task_4_1_infrastructure_ref_application
  pattern-regex: <ProjectReference\s+Include=.*CleanArchitecture\.Application.*\.csproj
  message: Add a project reference from the Infrastructure project to the Application project.
  languages: [generic]
  severity: WARNING

- id: task_4_1_efcore_inmemory_package
  pattern-regex: <PackageReference\s+Include\s*=\s*"Microsoft\.EntityFrameworkCore\.InMemory"
  message: Add the `Microsoft.EntityFrameworkCore.InMemory` NuGet package to the Infrastructure project.
  languages: [generic]
  severity: WARNING
```

---

Notice the dependency direction: Infrastructure references Application (and transitively Domain). It can see the contracts it needs to implement and the entities it needs to persist. But Application has no idea Infrastructure exists; it only knows about the interfaces.

### Task 4.2: Move and update the DbContext

The `RecipeCatalogDbContext` currently lives in the Web project at `Data/RecipeCatalogDbContext.cs`. It needs to move to the Infrastructure layer.

Create a new file `RecipeCatalogDbContext.cs` in the `Persistence` folder of the Infrastructure project. You can use the existing file in `Pluralsight.CleanArchitecture.Web/Data/RecipeCatalogDbContext.cs` as a starting point, but you need to make some changes:

1. Change the namespace to `Pluralsight.CleanArchitecture.Infrastructure.Persistence`.
2. Change the `using` statement to reference `Pluralsight.CleanArchitecture.Domain.Entities` instead of `Pluralsight.CleanArchitecture.Web.Models`.
3. The Fluent API configuration in `OnModelCreating` (keys, property constraints, `HasOne`/`WithMany`/`HasForeignKey`) stays the same — the Domain `Recipe` entity has the same `Category` navigation property.
4. In the `SeedData` method, the seed data for recipes needs a small adjustment. Because `DifficultyLevel` and `PrepTimeInMinutes` now have private setters, you can no longer set them in the object initializer. Remove those two lines from each recipe's initializer, and instead call `UpdatePreparation()` on each variable afterward. For example, the first recipe changes from:

```csharp
var pancakes = new Recipe
{
    Id = Guid.Parse("a0000001-0001-4000-a000-000000000001"),
    Title = "Classic Pancakes",
    Description = "Fluffy buttermilk pancakes served with maple syrup and fresh berries.",
    CategoryId = breakfastId,
    DifficultyLevel = 1,
    PrepTimeInMinutes = 20
};
```

to:

```csharp
var pancakes = new Recipe
{
    Id = Guid.Parse("a0000001-0001-4000-a000-000000000001"),
    Title = "Classic Pancakes",
    Description = "Fluffy buttermilk pancakes served with maple syrup and fresh berries.",
    CategoryId = breakfastId
};
pancakes.UpdatePreparation(1, 20);
```

Apply the same change to all eight recipe variables, using the original difficulty and prep time values. The `Category` seed data and the `HasData` calls do not need to change.

---
Check: Pluralsight.CleanArchitecture.Infrastructure/Persistence/RecipeCatalogDbContext.cs

```yaml
rules:
- id: task_4_2_dbcontext_namespace
  pattern-regex: namespace\s+Pluralsight\.CleanArchitecture\.Infrastructure\.Persistence
  message: Change the namespace to `Pluralsight.CleanArchitecture.Infrastructure.Persistence`.
  languages: [C#]
  severity: WARNING

- id: task_4_2_using_domain_entities
  pattern-regex: using\s+Pluralsight\.CleanArchitecture\.Domain\.Entities
  message: Add a `using` statement for `Pluralsight.CleanArchitecture.Domain.Entities`.
  languages: [C#]
  severity: WARNING

- id: task_4_2_seed_update_preparation
  pattern-regex: \.UpdatePreparation\s*\(
  message: Call `UpdatePreparation` on each recipe in the seed data to set difficulty and prep time.
  languages: [C#]
  severity: WARNING
```

---

This is a practical consequence of the private setters you added in Step 2. Even seed data must go through `UpdatePreparation()` to set the difficulty and prep time. The domain rules are enforced consistently, everywhere. The Fluent API configuration (`IsRequired`, `HasMaxLength`) replaces the data annotation attributes that were on the old model classes.

### Task 4.3: Implement the repository classes

Create `RecipeRepository.cs` in the `Persistence` folder. This class should implement `IRecipeRepository` and accept `RecipeCatalogDbContext` as a constructor parameter. You can refer to the existing `RecipesController` for the EF Core query patterns — the repository methods will contain much of the same data access code extracted from the controller actions.

Implement the methods:

- **GetAllAsync**: query `_context.Recipes` with `.Include(r => r.Category)` to eager-load the related category. Apply optional `Where` clauses if `categoryId` or `difficulty` are provided. Order by `Title` and return the result with `ToListAsync`.
- **GetByIdAsync**: use `_context.Recipes.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id)` to load the recipe with its category.
- **AddAsync**: add the recipe to `_context.Recipes`, call `SaveChangesAsync`, and return the recipe.
- **UpdateAsync**: call `_context.Recipes.Update(recipe)` then `SaveChangesAsync`.
- **DeleteAsync**: find the recipe by ID, and if it exists, remove it and call `SaveChangesAsync`.

Then create `CategoryRepository.cs` in the same folder. It should implement `ICategoryRepository` and accept `RecipeCatalogDbContext` as a constructor parameter.

- **GetAllAsync**: return all categories ordered by `Name` using `ToListAsync`.
- **GetByIdAsync**: use `_context.Categories.FindAsync(id)`.

Both classes will need `using` statements for `Microsoft.EntityFrameworkCore`, `Pluralsight.CleanArchitecture.Application.Contracts`, and `Pluralsight.CleanArchitecture.Domain.Entities`.

---
Check: Pluralsight.CleanArchitecture.Infrastructure/Persistence/RecipeRepository.cs

```yaml
rules:
- id: task_4_3_recipe_repository_class
  pattern-regex: public\s+class\s+RecipeRepository\s*:\s*IRecipeRepository
  message: Create a `RecipeRepository` class that implements `IRecipeRepository`.
  languages: [C#]
  severity: WARNING

- id: task_4_3_recipe_repository_include_category
  pattern-regex: \.Include\s*\(\s*r\s*=>\s*r\.Category\s*\)
  message: Use `.Include(r => r.Category)` to eager-load the category.
  languages: [C#]
  severity: WARNING
```

Check: Pluralsight.CleanArchitecture.Infrastructure/Persistence/CategoryRepository.cs

```yaml
rules:
- id: task_4_3_category_repository_class
  pattern-regex: public\s+class\s+CategoryRepository\s*:\s*ICategoryRepository
  message: Create a `CategoryRepository` class that implements `ICategoryRepository`.
  languages: [C#]
  severity: WARNING
```

---

The data access logic is the same as before, but now it lives in dedicated, focused classes behind clean interfaces rather than being scattered across controller actions.

### Task 4.4: Create the FileNotificationService

Create `FileNotificationService.cs` in the `Notifications` folder of the Infrastructure project. You will need a `using` statement for `Pluralsight.CleanArchitecture.Application.Contracts`. This class should implement `INotificationService` from the Application layer.

Implement the `SendNotificationAsync` method so that it writes a timestamped message to a file at `notifications/recipe-notifications.txt`. You can refer to the `File.AppendAllTextAsync` call in the existing `RecipesController.Create` action for the approach. Format each line as `[{timestamp}] {message}` followed by a newline, using `DateTime.UtcNow` formatted with the round-trip specifier (`"O"`).

---
Check: Pluralsight.CleanArchitecture.Infrastructure/Notifications/FileNotificationService.cs

```yaml
rules:
- id: task_4_4_filenotificationservice_class
  pattern-regex: public\s+class\s+FileNotificationService\s*:\s*INotificationService
  message: Create a `FileNotificationService` class that implements `INotificationService`.
  languages: [C#]
  severity: WARNING

- id: task_4_4_append_to_file
  pattern-regex: File\.AppendAllTextAsync
  message: Use `File.AppendAllTextAsync` to write the notification to a file.
  languages: [C#]
  severity: WARNING
```

---

### Task 4.5: Create the infrastructure service registration

Again we need to register the components defined in the project with the dependency injection container.

-

Create `InfrastructureServiceRegistration.cs` in the root of the Infrastructure project. You will need the following `using` statements:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.Extensions.DependencyInjection`
- `Pluralsight.CleanArchitecture.Application.Contracts`
- `Pluralsight.CleanArchitecture.Infrastructure.Notifications`
- `Pluralsight.CleanArchitecture.Infrastructure.Persistence`

Define a static class `InfrastructureServiceRegistration` in the `Pluralsight.CleanArchitecture.Infrastructure` namespace. Add a public static extension method on `IServiceCollection` called `AddInfrastructureServices` that returns `IServiceCollection`.

Inside the method, register the following:

- The `RecipeCatalogDbContext` using `AddDbContext` with the database name `"RecipeCatalog"`.
- `IRecipeRepository` to `RecipeRepository` as scoped.
- `ICategoryRepository` to `CategoryRepository` as scoped.
- `INotificationService` to `FileNotificationService` as singleton.

Return the `services` parameter to allow method chaining.

---
Check: Pluralsight.CleanArchitecture.Infrastructure/InfrastructureServiceRegistration.cs

```yaml
rules:
- id: task_4_5_infrastructure_registration_class
  pattern-regex: public\s+static\s+class\s+InfrastructureServiceRegistration
  message: Create a static class named `InfrastructureServiceRegistration`.
  languages: [C#]
  severity: WARNING

- id: task_4_5_add_infrastructure_services_method
  pattern-regex: public\s+static\s+IServiceCollection\s+AddInfrastructureServices\s*\(\s*this\s+IServiceCollection
  message: Add an `AddInfrastructureServices` extension method on `IServiceCollection`.
  languages: [C#]
  severity: WARNING

- id: task_4_5_register_dbcontext
  pattern-regex: AddDbContext<RecipeCatalogDbContext>
  message: Register `RecipeCatalogDbContext` using `AddDbContext`.
  languages: [C#]
  severity: WARNING

- id: task_4_5_register_recipe_repository
  pattern-regex: AddScoped<IRecipeRepository\s*,\s*RecipeRepository>
  message: Register `IRecipeRepository` to `RecipeRepository` as scoped.
  languages: [C#]
  severity: WARNING

- id: task_4_5_register_notification_service
  pattern-regex: AddSingleton<INotificationService\s*,\s*FileNotificationService>
  message: Register `INotificationService` to `FileNotificationService` as singleton.
  languages: [C#]
  severity: WARNING
```

---

With this registration class in place, the Web project will be able to wire up the entire Infrastructure layer with a single method call — just like the Application layer. The Web project's `Program.cs` will not need to know about `RecipeRepository`, `CategoryRepository`, or `FileNotificationService` directly.

## Step 5: Refactor the Web Layer

With the Domain, Application, and Infrastructure layers in place, it is time to bring everything together. The Web project is still the entry-point of the application, but it now becomes a much thinner shell. It handles HTTP requests, populates view models, and delegates everything else to the Application services. Anything that could be shared with another presentation — perhaps a public website, or an API — is migrated into an appropriate layer and can be reused.

The views and view models remain unchanged so the user will not notice any difference.

### Task 5.1: Update project references

First, remove the `Microsoft.EntityFrameworkCore.InMemory` package from the Web project. That dependency now belongs to the Infrastructure project:

```
dotnet remove Pluralsight.CleanArchitecture.Web/Pluralsight.CleanArchitecture.Web.csproj package Microsoft.EntityFrameworkCore.InMemory
```

Then add project references to the Application and Infrastructure projects:

```
dotnet add Pluralsight.CleanArchitecture.Web/Pluralsight.CleanArchitecture.Web.csproj reference Pluralsight.CleanArchitecture.Application/Pluralsight.CleanArchitecture.Application.csproj
dotnet add Pluralsight.CleanArchitecture.Web/Pluralsight.CleanArchitecture.Web.csproj reference Pluralsight.CleanArchitecture.Infrastructure/Pluralsight.CleanArchitecture.Infrastructure.csproj
```

---
Check: Pluralsight.CleanArchitecture.Web/Pluralsight.CleanArchitecture.Web.csproj

```yaml
rules:
- id: task_5_1_web_ref_application
  pattern-regex: <ProjectReference\s+Include=.*CleanArchitecture\.Application.*\.csproj
  message: Add a project reference from the Web project to the Application project.
  languages: [generic]
  severity: WARNING

- id: task_5_1_web_ref_infrastructure
  pattern-regex: <ProjectReference\s+Include=.*CleanArchitecture\.Infrastructure.*\.csproj
  message: Add a project reference from the Web project to the Infrastructure project.
  languages: [generic]
  severity: WARNING
```

---

The Web project references both Application and Infrastructure. This is because `Program.cs` is the **composition root** — the one place where all the concrete types are wired together. In a running application, the Web project needs to know about Infrastructure in order to register its services at startup, but the controllers themselves will only depend on the Application layer's interfaces.

### Task 5.2: Update Program.cs

Open `Pluralsight.CleanArchitecture.Web/Program.cs`.

Replace the `using` statements at the top. Remove `using Microsoft.EntityFrameworkCore` and `using Pluralsight.CleanArchitecture.Web.Data`. Add:

```csharp
using Pluralsight.CleanArchitecture.Application;
using Pluralsight.CleanArchitecture.Infrastructure;
using Pluralsight.CleanArchitecture.Infrastructure.Persistence;
```

Then replace the `AddDbContext` call:

```csharp
builder.Services.AddDbContext<RecipeCatalogDbContext>(options =>
    options.UseInMemoryDatabase("RecipeCatalog"));
```

with the two registration methods:

```csharp
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();
```

The `EnsureCreated` block that creates the database still needs to reference `RecipeCatalogDbContext` — that is why the `using` for `Pluralsight.CleanArchitecture.Infrastructure.Persistence` is needed. Everything else in `Program.cs` stays the same.

---
Check: Pluralsight.CleanArchitecture.Web/Program.cs

```yaml
rules:
- id: task_5_2_add_application_services_call
  pattern-regex: \.AddApplicationServices\s*\(\s*\)
  message: Call `AddApplicationServices()` in `Program.cs`.
  languages: [C#]
  severity: WARNING

- id: task_5_2_add_infrastructure_services_call
  pattern-regex: \.AddInfrastructureServices\s*\(\s*\)
  message: Call `AddInfrastructureServices()` in `Program.cs`.
  languages: [C#]
  severity: WARNING
```

---

This is the composition root pattern. All the concrete wiring happens here in `Program.cs`, and nowhere else. The controllers will receive their dependencies through constructor injection, without knowing which concrete classes implement them.

### Task 5.3: Putting the fat controller on a diet!

The existing recipes controller is what is sometimes called a **God class** — a single class that knows and does too much. In the next task you will strip much of of that away, leaving a thin controller that delegates to the Application services.

Open `Pluralsight.CleanArchitecture.Web/Controllers/RecipesController.cs`.

Replace the constructor so that instead of injecting `RecipeCatalogDbContext`, it injects `IRecipeService` and `ICategoryService`. Store them as private readonly fields.

Update the `using` statements: remove `Microsoft.EntityFrameworkCore` and `Pluralsight.CleanArchitecture.Web.Data`. Remove `Pluralsight.CleanArchitecture.Web.Models`. Add `Pluralsight.CleanArchitecture.Application.Services`.

Then rewrite each action to delegate to the services:

- **Index**: call `_recipeService.GetAllAsync(categoryId, difficulty)` and `_categoryService.GetAllAsync()`. Map the results to `RecipeIndexViewModel` the same way as before. Since the Recipe entity still has a `Category` navigation property, you can use `r.Category.Name` to get the category name.
- **Create (GET)**: populate a `RecipeFormViewModel` with the category select list, using `_categoryService.GetAllAsync()`.
- **Create (POST)**: if `ModelState.IsValid`, call `_recipeService.CreateAsync(...)` with the values from the view model. Wrap the call in a `try`/`catch` for `ArgumentException` — if caught, add the exception message to `ModelState` and return the view. On success, redirect to `Index`.
- **Edit (GET)**: call `_recipeService.GetByIdAsync(id)`. If null, return `NotFound()`. Otherwise populate a `RecipeFormViewModel`.
- **Edit (POST)**: if `ModelState.IsValid`, call `_recipeService.UpdateAsync(...)`. Use the same `try`/`catch` pattern as Create. On success, redirect to `Index`.
- **Delete (GET)**: call `_recipeService.GetByIdAsync(id)`. If null, return `NotFound()`. Populate a `RecipeDeleteViewModel` using `recipe.Category.Name` for the category name.
- **Delete (POST)**: call `_recipeService.DeleteAsync(id)` and redirect to `Index`.

You can also extract a private helper method `GetCategorySelectList()` that calls `_categoryService.GetAllAsync()` and maps the results to `SelectListItem` objects. This avoids repeating that mapping in multiple actions.

Remove all direct `_context` usage, all inline validation logic, and all `File.AppendAllTextAsync` calls. The controller should have no `using` for `Microsoft.EntityFrameworkCore` and no reference to `RecipeCatalogDbContext`.

---
Check: Pluralsight.CleanArchitecture.Web/Controllers/RecipesController.cs

```yaml
rules:
- id: task_5_3_inject_recipe_service
  pattern-regex: IRecipeService\s+\w+
  message: Inject `IRecipeService` into the controller.
  languages: [C#]
  severity: WARNING

- id: task_5_3_inject_category_service
  pattern-regex: ICategoryService\s+\w+
  message: Inject `ICategoryService` into the controller.
  languages: [C#]
  severity: WARNING

- id: task_5_3_using_application_services
  pattern-regex: using\s+Pluralsight\.CleanArchitecture\.Application\.Services
  message: Add a `using` statement for `Pluralsight.CleanArchitecture.Application.Services`.
  languages: [C#]
  severity: WARNING

- id: task_5_3_call_create_async
  pattern-regex: _recipeService\.CreateAsync\s*\(
  message: Call `_recipeService.CreateAsync` in the Create POST action.
  languages: [C#]
  severity: WARNING

- id: task_5_3_call_update_async
  pattern-regex: _recipeService\.UpdateAsync\s*\(
  message: Call `_recipeService.UpdateAsync` in the Edit POST action.
  languages: [C#]
  severity: WARNING

- id: task_5_3_call_delete_async
  pattern-regex: _recipeService\.DeleteAsync\s*\(
  message: Call `_recipeService.DeleteAsync` in the Delete POST action.
  languages: [C#]
  severity: WARNING

- id: task_5_3_catch_argument_exception
  pattern-regex: catch\s*\(\s*ArgumentException
  message: Catch `ArgumentException` from service calls and add the error to `ModelState`.
  languages: [C#]
  severity: WARNING
```

---

The controller is now a **humble object**. It translates between HTTP and the Application layer — nothing more. All business logic, validation, data access, and notification logic have moved to the layers where they belong, and can more easily be tested. The controller is so thin it barely needs testing.

### Task 5.4: Remove old files and update view imports

Now that the Web project uses the Domain entities via the Application and Infrastructure layers, the old model and data access classes are no longer needed.

Delete the following files from the Web project:

- `Data/RecipeCatalogDbContext.cs`
- `Models/Recipe.cs`
- `Models/Category.cs`

You can also delete the now-empty `Data` and `Models` folders.

Then open `Views/_ViewImports.cshtml` and remove the line:

```
@using Pluralsight.CleanArchitecture.Web.Models
```

This namespace no longer exists. The views only reference view model types from `Pluralsight.CleanArchitecture.Web.ViewModels`, which is already imported.

Finally, verify that everything compiles:

```
dotnet build
```

---
Check: Pluralsight.CleanArchitecture.Web/Views/_ViewImports.cshtml

```yaml
rules:
- id: task_5_4_viewimports_no_models
  pattern-regex: "@using\\s+Pluralsight\\.CleanArchitecture\\.Web\\.ViewModels"
  message: Ensure `_ViewImports.cshtml` imports `Pluralsight.CleanArchitecture.Web.ViewModels`.
  languages: [generic]
  severity: WARNING
```

---

If the build succeeds, the refactoring to clean architecture is complete. You have gone from one project to four, with every dependency correctly pointing inward.

## Step 6: Verify and Sum Up

### Verification

Run the application:

```
cd Pluralsight.CleanArchitecture.Web
dotnet run
```

Open the web browser and verify each of the following:

1. The recipe list displays the seeded recipes.
2. Filtering by category shows only recipes in that category.
3. Filtering by difficulty shows only recipes at that level.
4. Creating a new recipe works. After creating one, check that a `notifications/recipe-notifications.txt` file has been created in the project root with a timestamped entry.
5. Editing an existing recipe works.
6. Deleting a recipe works.
7. Try creating a recipe with a difficulty of 1 and a prep time over 60 minutes. The application should reject it with a validation error.

The application behaves identically to when you first ran it in Step 1. The difference is entirely in the code behind it.

### What You Have Built

You started with a single project where one controller handled everything: HTTP requests, database queries, file I/O, business validation, and view model mapping. You now have four projects constructed according to clean architecture principles, each with a clear responsibility:

- **Domain** holds the business rules. Entities protect their own invariants and have no dependencies on any framework.
- **Application** defines what the application can do through service interfaces and orchestrates business logic. It depends only on Domain.
- **Infrastructure** implements the Application layer's contracts with concrete technologies: EF Core for data access, the filesystem for notifications. It can be swapped without touching business logic.
- **Web** is a thin shell that translates between HTTP and the Application services. The controllers are humble objects with minimal logic.

Every dependency points inward. The inner layers have no knowledge of the outer layers, and each layer can evolve independently.

### Where to Go from Here

This lab covered the foundations of clean architecture on what of course is a very simple application. There is more you can build on top of this structure as an application grows in complexity.

**Testability.** One of the most significant benefits of what you have built is how much easier the code is to test. Consider what it would take to write automated test for the original `RecipesController`: you would need a database, a real filesystem for notifications, and an HTTP context. To test `RecipeService` you can mock `IRecipeRepository`, `ICategoryRepository`, and `INotificationService`, and test the business logic in isolation. The domain is even simpler. `Recipe.UpdatePreparation()` can be tested with a plain unit test that verifies invalid values throw exceptions and valid values are assigned. Clean architecture makes testing easy by design.

**Richer domain modeling.** The `UpdatePreparation` method was a first step toward a richer domain model. In more complex systems, domain-driven design (DDD) goes further with concepts like **value objects** (e.g., a `DifficultyLevel` type that can never hold an invalid value), **aggregates** (clusters of entities that enforce consistency boundaries), and **domain events** (e.g., a `RecipeCreatedEvent` that triggers the notification instead of the service calling it directly). These techniques keep business logic expressive and centralized as complexity grows.

**The Mediator pattern and CQRS.** Instead of controllers calling service classes directly, you can use a library like **MediatR** to dispatch requests to handlers. Each handler is a self-contained class for a single operation, such as `CreateRecipeCommandHandler`. This enforces **Command Query Responsibility Segregation (CQRS)**, where reads and writes become distinct types. It also strengthens the **Open/Closed Principle** (the "O" in SOLID). Adding a new feature means adding a new handler class, never modifying existing ones.

**Validation frameworks.** The validation in `RecipeService` is hand-written. Libraries like **FluentValidation** let you define validation rules in dedicated classes that are automatically invoked before your service logic runs, keeping validation concerns separate and reusable.

**Cross-cutting concerns.** As the application grows, you will want consistent behavior across all operations: global error handling that converts exceptions to appropriate HTTP responses, structured logging with a library like **Serilog**, and pipeline behaviors for concerns like caching or performance monitoring. Clean architecture gives these concerns natural places to live without polluting business logic.

**Swapping infrastructure.** What would it take to swap the in-memory database for SQL Server or PostgreSQL? Only a change to the Infrastructure project — a different EF Core provider package and a connection string. The Application and Domain layers would not change at all. That is the promise of clean architecture: the database, the web framework, and the notification mechanism are all details that can be changed independently of the business rules they serve.
