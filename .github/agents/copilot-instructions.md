# ContosoDashboard Development Guidelines

Auto-generated from active feature plans. Last updated: 2026-06-26

## Active Technologies

- C# 12 on ASP.NET Core 8 / Blazor Server (`net8.0`)
- Entity Framework Core with SQLite local persistence
- Cookie-based mock authentication and role-based authorization
- Bootstrap 5 / Bootstrap Icons for UI

## Project Structure

```text
ContosoDashboard/
├── Data/       # EF Core DbContext and data configuration
├── Models/     # Entity models and enums
├── Pages/      # Blazor pages and Razor login/logout endpoints
├── Services/   # Business logic, authorization, orchestration
├── Shared/     # Reusable UI components/layout
└── wwwroot/    # Static assets only; protected files must stay outside this path

specs/001-document-upload-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
└── contracts/
```

## Commands

- Run the app: `cd ContosoDashboard && dotnet run`

## Code Style

- Keep UI coordination in `Pages/` and `Shared/`.
- Keep business rules, authorization, and workflow orchestration in `Services/`.
- Route persistence through `ApplicationDbContext` and SQLite-aligned EF Core models.
- Use DI-backed abstractions for local file storage and other replaceable infrastructure.
- Enforce fail-closed service-level authorization for document, project, task, and notification data.

## Recent Changes

- `001-document-upload-management`: planned document metadata, local file storage abstraction,
  read-only sharing, audit/reporting, and dashboard/project/task/notification integration.

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
