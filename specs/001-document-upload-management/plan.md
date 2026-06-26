# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-06-26 | **Spec**: `/Users/mauriciovillegas/development/coderoad/ContosoDashboard-SSD./specs/001-document-upload-management/spec.md`
**Input**: Feature specification from `/Users/mauriciovillegas/development/coderoad/ContosoDashboard-SSD./specs/001-document-upload-management/spec.md`

## Summary

Add an offline-first document management capability to the existing ASP.NET Core 8 Blazor Server
training app by introducing SQLite-backed document metadata, local file storage and safety
verification abstractions, service-level authorization for owner/team/project/share/admin access,
and UI/reporting integrations across dashboard, project, task, and notification experiences.

## Technical Context

**Language/Version**: C# 12 on ASP.NET Core 8 / Blazor Server (`net8.0`)  
**Primary Dependencies**: ASP.NET Core 8, Blazor Server, Entity Framework Core Sqlite,
cookie-based mock authentication, Bootstrap 5, Bootstrap Icons  
**Storage**: SQLite `ContosoDashboard.db` for metadata/shares/audit records plus local filesystem
storage outside `wwwroot` under app content root (`AppData/uploads/...`) for document binaries  
**Testing**: Repository currently has no test project; plan for manual role-based acceptance and
negative-path verification via the seeded mock users, with optional implementation-phase addition
of targeted xUnit/bUnit coverage for document services and high-risk UI flows  
**Target Platform**: Local macOS/Windows/Linux development environments running the existing
Blazor Server application offline  
**Project Type**: Single-project ASP.NET Core web application (`ContosoDashboard/`)  
**Performance Goals**: 95% of <=25 MB uploads complete within 30 seconds; 95% of list/search
results for up to 500 accessible documents complete within 2 seconds; 95% of previewable
documents open within 3 seconds  
**Constraints**: Training-only behavior, offline-first operation, fail-closed service-level
authorization and IDOR protection, SQLite-aligned persistence, local file storage outside
`wwwroot`, read-only explicit shares, task/project context rules, and DI-backed abstractions for
future cloud migration  
**Scale/Scope**: Seeded training data plus document management for the current four mock roles,
multi-file uploads up to 25 MB each, dashboard recent-document summaries, project/task integration,
notification fan-out, and administrator audit/reporting for hundreds of documents per user

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Initial gate status: PASS**

- **Training-only scope**: The feature remains a local training workflow. Document safety checking,
  storage, sharing, reporting, and authorization are designed as training implementations with
  optional production migration guidance only through abstractions such as
  `IFileStorageService` and a local safety-verification service.
- **Offline-first behavior**: Core upload, browse, search, preview, download, share, notification,
  and audit flows will run without mandatory internet or cloud services. Files are stored locally,
  metadata lives in SQLite, and any future cloud path remains optional behind DI.
- **Authorization boundary**:
  - Employee: may upload personal documents and project documents only for projects they belong to;
    may browse/download/preview only documents they own, receive by share, or gain through current
    project/task membership.
  - Team Lead: inherits employee rights and may manage documents uploaded by users in the same
    department, which is the repository's current proxy for "team".
  - Project Manager: may manage documents associated with projects they manage and may upload from
    project/task contexts for those projects.
  - Administrator: full document access for browse, management, audit, and reporting.
  - Explicit share: grants read-only browse/search/preview/download access and never grants edit,
    replace, delete, or resharing rights by itself.
  - Archived/completed/deleted project or task contexts stop acting as a visibility path; access
    falls back to any still-valid owner/share/project/admin entitlement.
  - Every document service method must re-check the current requesting user before returning
    metadata, file content, or mutation results so direct object references fail closed.
- **Architecture boundary**:
  - `Pages/`: add a documents experience plus targeted updates to `Index.razor`,
    `ProjectDetails.razor`, `Tasks.razor`, `Notifications.razor`, and navigation.
  - `Shared/`: reusable upload/share/filter/preview components stay presentation-only.
  - `Services/`: new `IDocumentService`, `IFileStorageService`, and local safety-verification
    abstraction hold business rules, authorization, workflow orchestration, and reporting logic.
  - `Models/`: add document, tag, share, and activity/audit entities aligned to existing integer
    keys and optional project/task relationships.
  - `Data/`: extend `ApplicationDbContext` with new `DbSet<>` entries, relationships, and indexes.
  - `Program.cs`: register new services/options without bypassing the service layer.
- **Persistence and verification**: SQLite remains the local source of truth for metadata, shares,
  and audit trails; binaries stay in local storage outside `wwwroot`. The plan includes setup/reset
  notes for database and upload folders plus verification for happy paths, upload rejection, role
  isolation, share restrictions, inactive-context visibility, and administrator-only reporting.

**Post-design re-check status: PASS**

- Research and design keep all core behaviors local and self-contained.
- Document access remains service-authorized at every list, detail, preview, download, mutate, and
  report path.
- New infrastructure is limited to DI-backed local implementations that preserve future migration
  flexibility without introducing mandatory cloud dependencies.
- SQLite and local filesystem reset implications are documented in `quickstart.md`, and contracts
  reflect fail-closed record access semantics.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── document-management.openapi.yaml
└── tasks.md                # Phase 2 output, not created by this plan step
```

### Source Code (repository root)

```text
ContosoDashboard/
├── Data/
├── Models/
├── Pages/
├── Services/
├── Shared/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── ContosoDashboard.csproj

StakeholderDocs/
README.md
.specify/
```

**Structure Decision**: Keep all implementation inside the existing `ContosoDashboard/` Blazor
Server app. Add UI pages/components in `Pages/` and `Shared/`, place all document authorization and
workflow logic in `Services/`, persist entities through `Data/ApplicationDbContext`, and register
local training implementations in `Program.cs`. Store uploaded files under app content root outside
`wwwroot` so every preview/download request passes through an authorized application path.

## Phase 0 Research Focus

- Confirm the local storage path, upload sequence, and DI abstraction pattern for document binaries.
- Confirm a training-safe offline verification approach for harmful-content checks that can fail
  closed without cloud or third-party dependencies.
- Confirm how "team" maps onto the current repository model; use shared `Department` membership as
  the planning baseline because the repo has no standalone team entity.
- Confirm role/ownership/share/inactive-context authorization rules across document list, detail,
  search, preview, download, replace, delete, and reporting flows.
- Confirm where dashboard, task, project, and notification integrations fit in the current UI.
- Confirm a repository-appropriate validation strategy given the current absence of an automated
  test project.

## Phase 1 Design Focus

- Model documents, tags, shares, and document activity records with integer keys and SQLite-ready
  relationships.
- Define local storage, preview/download, share, and audit/reporting contracts.
- Document manual verification and reset steps for the local training environment.
- Update agent context only for new, relevant planning concepts introduced by this feature.

## Complexity Tracking

No constitution violations or exceptional complexity justifications are required at plan time.
