# Research: Document Upload and Management

## Decision 1: Use a local file storage abstraction backed by `AppData/uploads`

- **Decision**: Introduce `IFileStorageService` with a training implementation
  `LocalFileStorageService` that stores files outside `wwwroot` under the app content root using a
  relative path pattern like `{uploaderUserId}/{projectId-or-personal}/{documentGuid}.{extension}`.
  The upload sequence will be: validate input -> authorize requester -> verify file safety ->
  generate path -> save file -> persist metadata -> emit notifications/audit entry.
- **Rationale**: This matches stakeholder guidance, keeps the app fully offline, avoids exposing
  document binaries directly from static files, and preserves a migration seam for future cloud
  storage without changing document business logic.
- **Alternatives considered**:
  - Store files in `wwwroot`: rejected because it would bypass authorization at the point of file
    access.
  - Store binary content inside SQLite: rejected because it complicates the current training app and
    adds unnecessary SQLite size/performance pressure for 25 MB uploads.
  - Introduce Azure Blob Storage now: rejected because mandatory cloud dependencies violate the
    offline-first constitution.

## Decision 2: Implement training-scope offline safety verification behind a service abstraction

- **Decision**: Add an offline `IDocumentSafetyService` (or equivalent) that performs deterministic
  training-safe validation before availability: extension allowlist, MIME/content-type validation,
  basic file signature checks for known types, stream readability checks, and fail-closed handling
  whenever verification cannot complete.
- **Rationale**: The spec requires pre-availability harmful-content verification, but the
  repository's constitution forbids mandatory cloud or third-party runtime dependencies. A local
  safety-verification abstraction satisfies training expectations while making it explicit that
  production-grade malware scanning would be an optional future implementation.
- **Alternatives considered**:
  - Skip safety verification: rejected because it violates FR-008.
  - Call an online antivirus or malware API: rejected because it breaks offline-first behavior.
  - Claim full production-grade malware scanning from simple extension checks: rejected because the
    repository must remain honest about its training-only scope.

## Decision 3: Centralize all entitlement logic in `DocumentService`

- **Decision**: Create `IDocumentService`/`DocumentService` as the sole orchestration layer for
  upload, browse, search, preview, download, edit, replace, delete, share, task/project
  integration, dashboard summaries, and admin reporting. Every method will accept the requesting
  user and evaluate owner, department/team, project membership/ownership, explicit share, and admin
  rights before returning data or file content.
- **Rationale**: Existing repository services already enforce authorization in the service layer by
  returning null/false for unauthorized access. Reusing that pattern provides defense in depth and
  directly addresses the constitution's IDOR protection requirements.
- **Alternatives considered**:
  - Enforce rules only in Blazor pages/components: rejected because UI gating alone is insufficient
    and vulnerable to direct object references.
  - Split authorization into multiple unrelated helper services first: rejected for plan scope
    because it adds indirection before the document rules are stable.

## Decision 4: Treat "team" sharing/management as department-based in v1

- **Decision**: Model team-based management and share targets using the repository's existing
  department concept. `DocumentShare` will support either a direct user target or a department/team
  target, and Team Lead management rights will apply to documents uploaded by users in the same
  department.
- **Rationale**: The current codebase has no standalone Team entity, while `User.Department` and the
  `UserService.GetTeamMembersAsync` flow already act as the effective team boundary in the training
  app. Reusing that concept avoids unnecessary schema expansion while still meeting the stakeholder
  intent.
- **Alternatives considered**:
  - Create full Team and TeamMembership tables now: rejected because it expands scope beyond what
    the repository currently models.
  - Allow only direct-user shares: rejected because the stakeholder requirements explicitly mention
    sharing with teams.

## Decision 5: Integrate document UX into the existing dashboard, project, task, and notification surfaces

- **Decision**: Add a top-level documents experience for My Documents, Shared with Me, and search;
  update `Index.razor` with recent documents and a document count; extend project and task contexts
  to surface related documents; and reuse `NotificationService` for document share and project
  upload notifications.
- **Rationale**: These integrations align to current page structure and directly satisfy the
  feature's value proposition without requiring a separate subsystem. Project and task pages already
  expose the contextual places where users expect related documents.
- **Alternatives considered**:
  - Ship a documents page with no dashboard/project/task integration: rejected because it misses
    FR-018 through FR-020.
  - Build a separate admin-only document module first: rejected because upload/access workflows are
    higher-priority user stories.

## Decision 6: Use manual role-based verification as the baseline and document a future automated-test gap

- **Decision**: Plan manual acceptance and unauthorized-path verification using the seeded mock
  users, local SQLite database, and local upload folder reset steps. Document a follow-up option to
  add an automated test project during implementation if the feature complexity warrants it.
- **Rationale**: The repository currently contains no test project, but the feature has meaningful
  authorization and storage risk. Explicit manual verification criteria keep plan scope realistic
  while acknowledging where future automation would add value.
- **Alternatives considered**:
  - Promise full automated coverage in the initial plan: rejected because no test harness currently
    exists.
  - Rely only on ad hoc spot checks: rejected because the feature includes high-risk unauthorized
    access and delete/reporting paths that require explicit verification.

## Decision 7: Preserve audit history separately from active document records

- **Decision**: Keep active document metadata in `Document` plus related share/tag rows, but store
  durable audit/reporting data in `DocumentActivityRecord` with enough snapshot fields to remain
  useful after a document is deleted and its active shares are revoked.
- **Rationale**: The spec requires permanent removal from user-facing access while preserving audit
  history for administrator review. A separate audit record allows hard deletion of user-facing
  records without losing reporting context.
- **Alternatives considered**:
  - Soft-delete documents as the primary retention approach: rejected because soft-delete/recycle-bin
    behavior is explicitly outside the feature scope.
  - Delete all traces of a document, including activity history: rejected because it violates FR-021
    and FR-028.
