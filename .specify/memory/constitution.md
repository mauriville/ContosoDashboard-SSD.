<!--
Sync Impact Report
- Version change: template/unversioned -> 1.0.0
- Modified principles:
  - Placeholder principle 1 -> I. Training-Only Scope
  - Placeholder principle 2 -> II. Offline-First Self-Containment
  - Placeholder principle 3 -> III. Service-Level Authorization and IDOR Protection
  - Placeholder principle 4 -> IV. Layered Architecture with Dependency Injection
  - Placeholder principle 5 -> V. Verifiable, SQLite-Aligned Changes
- Added sections:
  - Operational Constraints
  - Development Workflow
- Removed sections:
  - None
- Templates requiring updates:
  - ✅ updated .specify/templates/plan-template.md
  - ✅ updated .specify/templates/spec-template.md
  - ✅ updated .specify/templates/tasks-template.md
  - ⚠ pending .specify/templates/commands/*.md (directory not present in repository)
- Follow-up TODOs:
  - None
-->
# ContosoDashboard Constitution

## Core Principles

### I. Training-Only Scope
- ContosoDashboard MUST remain an educational, non-production application.
- Features, code samples, and documentation MUST preserve the training context and MUST label
  mock authentication, sample data, and simplified operations as training implementations.
- Production migration guidance MAY be documented, but production identity, compliance, or cloud
  dependencies MUST NOT become required to run, test, or teach the current application locally.

Rationale: the repository exists to teach spec-driven development and secure architectural patterns
without requiring enterprise infrastructure or production hardening.

### II. Offline-First Self-Containment
- All core user journeys MUST work locally without internet access or mandatory external services.
- New integrations MUST default to local, self-contained implementations for training use.
- Future cloud migration paths MAY be prepared, but they MUST be introduced behind interfaces and
  dependency injection so the training implementation remains fully functional offline.

Rationale: the project is explicitly designed for local training availability and must stay usable
without subscriptions, service accounts, or network dependencies.

### III. Service-Level Authorization and IDOR Protection
- Every protected page, endpoint, or interactive workflow MUST enforce the appropriate ASP.NET Core
  authorization rule at the entry point.
- Every service method that reads or mutates user-scoped, task-scoped, project-scoped,
  notification-scoped, or document-scoped data MUST validate the requesting user's entitlement
  before returning data or applying changes.
- Unauthorized requests MUST fail closed and MUST NOT disclose protected record contents or
  resource existence beyond the minimum needed for UX handling.
- Specs, plans, and verification notes MUST identify affected roles, ownership rules, and
  unauthorized access paths for each security-sensitive change.

Rationale: the codebase teaches defense in depth, and service-level checks are the primary control
that prevents IDOR and cross-user data exposure.

### IV. Layered Architecture with Dependency Injection
- UI components and pages MUST coordinate presentation only; business rules, authorization, and
  orchestration MUST live in services.
- Persistence MUST flow through EF Core and `ApplicationDbContext`; direct data access that bypasses
  the service layer MUST be limited to startup, seeding, or an explicitly justified exception.
- Infrastructure variability, including file storage or future cloud integrations, MUST be modeled
  through interfaces and dependency injection so implementations can be swapped without rewriting
  business logic.
- Changes MUST preserve clear boundaries among `Pages`/`Shared`, `Services`, `Models`, and `Data`.

Rationale: clean separation of concerns keeps the training application understandable, testable, and
aligned with the architecture described in the README and stakeholder guidance.

### V. Verifiable, SQLite-Aligned Changes
- New persistence work MUST target EF Core with SQLite as the default local development database.
- Proposals that depend on SQL Server LocalDB or another mandatory local database runtime are
  non-compliant unless this constitution is amended first.
- Changes that affect schema, seed data, or file-backed storage MUST document setup, reset, and
  migration impacts in the relevant spec, plan, quickstart, or README updates.
- Every feature MUST include verification proportional to risk, including explicit checks for
  unauthorized access when protected data is involved and local/offline behavior when storage or
  infrastructure changes.

Rationale: the repository recently standardized on SQLite for local development, and predictable
local setup is essential for repeatable training exercises.

## Operational Constraints

- **Technology baseline**: Feature work MUST assume ASP.NET Core 8, Blazor Server, EF Core, SQLite,
  Bootstrap, and the existing role hierarchy unless a constitution amendment approves a change.
- **Authentication baseline**: Training builds MUST use the current mock cookie-based
  authentication and role-based authorization model. Production identity providers MAY be described
  only as optional migration guidance.
- **Storage baseline**: Local data and file storage MUST remain self-contained. Sensitive uploaded
  artifacts MUST be stored outside publicly served paths and exposed only through authorized
  application flows.
- **Authoritative context**: `README.md` and `StakeholderDocs/` MUST be reviewed during
  specification and planning. Requirements from those sources that affect offline behavior,
  authorization, storage, or architecture MUST be reflected in the resulting artifacts.

## Development Workflow

- Specifications MUST capture user roles, authorization boundaries, offline expectations,
  persistence or storage impacts, and any migration-path abstractions introduced for the feature.
- Implementation plans MUST pass a Constitution Check before design proceeds and MUST name affected
  pages, services, models, data components, dependency injection registrations, and SQLite or seed
  data impacts.
- Task breakdowns MUST include the work needed to enforce service-level authorization, validate
  unauthorized paths, update local setup/reset guidance, and preserve architectural boundaries when
  those concerns are affected.
- Code review and self-review MUST reject changes that add mandatory cloud dependencies, bypass
  service authorization, mix UI with business logic, or assume non-SQLite local persistence.

## Governance

- This constitution overrides conflicting local habits, plan notes, and task shortcuts.
- Amendments MUST be made in the same change set as any dependent template or documentation updates
  needed to keep the workflow consistent.
- Amendment proposals MUST include a summary of the change, the rationale, the impacted principles
  or sections, and any migration or follow-up work required.
- Semantic versioning applies to this constitution: MAJOR for incompatible governance or principle
  changes, MINOR for new principles or materially expanded guidance, and PATCH for clarifications or
  wording-only refinements.
- Every feature plan and review MUST perform an explicit constitution compliance check, and every
  PR that affects architecture, auth, storage, or workflow MUST verify alignment before merge.

**Version**: 1.0.0 | **Ratified**: 2026-06-26 | **Last Amended**: 2026-06-26
