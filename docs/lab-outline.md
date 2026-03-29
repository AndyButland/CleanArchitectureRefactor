# Guided: Refactoring to Clean Architecture with ASP.NET Core 10

## Concepts

1. Analyze an existing ASP.NET Core 10 application to identify violations of Clean Architecture principles, including mixed concerns in controllers, tight coupling to infrastructure, and duplicated logic.
2. Restructure the application into domain, application, infrastructure, and presentation layers with clearly defined boundaries and inward-pointing dependencies.
3. Apply the Dependency Inversion Principle by defining contracts in the application layer and implementing them in the infrastructure layer, decoupling business logic from databases and external services.
4. Encapsulate business rules within domain entities using rich domain model techniques, ensuring entities protect their own invariants.

## Audience Profile

Intermediate ASP.NET Core developers who are comfortable building working applications but want to learn how to structure them for long-term maintainability, testability, and separation of concerns.

## Abstract

A working application can be easy to build. A maintainable one - where the effort to add features stays low throughout the lifetime of the project - well, that takes more deliberate effort. In this code lab you will take a fully functional ASP.NET Core 10 Recipe Catalog application and refactor it from a single-project design — where controllers handle everything from validation to database queries to file I/O — into a clean architecture with clearly separated layers. You will create a domain layer with entities that enforce their own business rules, an application layer that defines service contracts and orchestrates logic, and an infrastructure layer that implements data access and external services behind those contracts. By the end, the application will work exactly as before, but the code behind it will be structured for testability, flexibility, and growth.

## Description

You have been brought in to improve the architecture of a Recipe Catalog application built with ASP.NET Core 10. The application works — users can browse, filter, create, edit, and delete recipes through a web interface — but all of the logic lives in a single project with fat controllers that directly access the database, write to the filesystem, and duplicate validation code.

Your job is to refactor this application into a clean architecture without changing its external behavior. The user interface will continue to work identically throughout.

Starting from the existing single-project application, you will:

- Explore the current codebase to identify architectural problems: mixed responsibilities in controllers, tight coupling to Entity Framework Core and the filesystem, duplicated validation logic, and the absence of any layered boundaries.
- Create a **Domain** layer containing clean entity classes with no framework dependencies. The Recipe entity will use private setters and dedicated methods to ensure its properties can never be set to invalid values.
- Create an **Application** layer that defines repository and service contracts as interfaces, with service classes that orchestrate business logic — validation, data access, and notifications — without any knowledge of how those operations are implemented.
- Create an **Infrastructure** layer that sits at the application's external boundary and implements the application contracts. This is where the application interacts with the outside world via input/output (I/O) operations: databases, filesystems and network services.
- Refactor the **Web** layer by replacing the fat controller with a thin one that delegates all work to application services, wiring everything together through dependency injection.
- Verify that the refactored application behaves identically to the original by running it and exercising all features through the user interface.