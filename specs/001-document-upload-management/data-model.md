# Data Model: Document Upload and Management

## Overview

The feature adds document metadata, sharing, tags, and audit/reporting entities to the existing
SQLite-backed EF Core model. All new entities use integer primary keys to stay aligned with the
current `User`, `Project`, and `TaskItem` design.

## Entity: Document

**Purpose**: Active user-facing metadata for a stored document.

| Field | Type | Notes / Validation |
|---|---|---|
| `DocumentId` | `int` | Primary key |
| `Title` | `string` | Required, 1-200 chars |
| `Description` | `string?` | Optional, up to 2000 chars |
| `Category` | `string` | Required; one of `Project Documents`, `Team Resources`, `Personal Files`, `Reports`, `Presentations`, `Other` |
| `OriginalFileName` | `string` | Required, up to 255 chars |
| `StoredFilePath` | `string` | Required relative path, up to 500 chars, unique among active documents |
| `FileType` | `string` | Required MIME/content type, up to 255 chars |
| `FileExtension` | `string` | Required normalized extension from allowlist |
| `FileSizeBytes` | `long` | Required, `> 0` and `<= 26,214,400` |
| `StorageProvider` | `string` | Required; default `LocalFileSystem` |
| `UploaderUserId` | `int` | Required FK to `User`; owner/uploader for personal ownership rules |
| `ProjectId` | `int?` | Optional FK to `Project`; required when document is uploaded in project/task context |
| `TaskId` | `int?` | Optional FK to `TaskItem`; if set, task must be active and belong to `ProjectId` |
| `UploadedAtUtc` | `DateTime` | Required |
| `UpdatedAtUtc` | `DateTime` | Required |
| `LastContentUpdatedAtUtc` | `DateTime?` | Set when file replacement succeeds |

**Relationships**

- `User (1) -> (many) Document` through `UploaderUserId`
- `Project (0..1) -> (many) Document`
- `TaskItem (0..1) -> (many) Document`
- `Document (1) -> (many) DocumentTag`
- `Document (1) -> (many) DocumentShare`
- `Document (1) -> (many) DocumentActivityRecord`

**Validation rules**

- Title and category are mandatory for every upload and metadata update.
- File type/extension must be in the supported allowlist.
- File size must be `<= 25 MB`.
- `TaskId` implies `ProjectId` and the referenced task's `ProjectId` must match.
- Upload to a project requires current project membership or project-manager/admin authority.
- If the linked project is completed/archived or the task is completed/inactive, the document record
  may remain active, but that context no longer grants browse visibility by itself.

## Entity: DocumentTag

**Purpose**: Searchable user-defined tags for documents.

| Field | Type | Notes / Validation |
|---|---|---|
| `DocumentTagId` | `int` | Primary key |
| `DocumentId` | `int` | Required FK to `Document` |
| `TagValue` | `string` | Required, normalized, 1-50 chars |
| `CreatedAtUtc` | `DateTime` | Required |

**Validation rules**

- Tag values are trimmed and normalized for case-insensitive search.
- A document cannot have duplicate normalized tags.

## Entity: DocumentShare

**Purpose**: Read-only access grants outside direct ownership.

| Field | Type | Notes / Validation |
|---|---|---|
| `DocumentShareId` | `int` | Primary key |
| `DocumentId` | `int` | Required FK to `Document` |
| `SharedByUserId` | `int` | Required FK to `User` |
| `SharedWithUserId` | `int?` | Direct user share target |
| `SharedWithDepartment` | `string?` | Department/team share target for training app v1 |
| `AccessLevel` | `string` | Required; fixed to `Read` in v1 |
| `CreatedAtUtc` | `DateTime` | Required |
| `RevokedAtUtc` | `DateTime?` | Set when access is revoked or document is deleted |

**Validation rules**

- Exactly one of `SharedWithUserId` or `SharedWithDepartment` must be populated.
- `AccessLevel` is always `Read`; it never grants edit, replace, delete, or resharing rights.
- Only the document owner, an authorized Team Lead, the relevant Project Manager, or an
  Administrator may create or revoke a share.
- Deleted documents revoke all active shares.

## Entity: DocumentActivityRecord

**Purpose**: Durable audit/reporting history for uploads, downloads, shares, updates, replacements,
deletions, and rejected safety/authorization attempts.

| Field | Type | Notes / Validation |
|---|---|---|
| `DocumentActivityRecordId` | `int` | Primary key |
| `DocumentId` | `int?` | Nullable FK to `Document`; may remain null after document deletion |
| `ActorUserId` | `int` | Required FK to `User` |
| `ActionType` | `string` | Required; e.g. `Upload`, `Download`, `Share`, `Delete`, `Replace`, `MetadataUpdate`, `UploadRejected`, `AccessDenied` |
| `Outcome` | `string` | Required; e.g. `Succeeded`, `Rejected`, `Denied`, `Failed` |
| `OccurredAtUtc` | `DateTime` | Required |
| `ProjectId` | `int?` | Optional contextual FK |
| `TaskId` | `int?` | Optional contextual FK |
| `ShareTargetUserId` | `int?` | Optional contextual FK |
| `ShareTargetDepartment` | `string?` | Optional department/team context |
| `DocumentTitleSnapshot` | `string` | Required snapshot for audit after deletion |
| `DocumentCategorySnapshot` | `string?` | Optional snapshot |
| `DocumentFileTypeSnapshot` | `string?` | Optional snapshot for reporting |
| `ContextSummary` | `string?` | Optional human-readable note or share/project context |

**Validation rules**

- Every audited action must record actor, action, timestamp, outcome, and relevant project/share
  context when available.
- Admin reports aggregate from this table rather than from active document rows only.
- Document deletion preserves activity history even after the file and active shares are removed.

## Derived / Existing Model Impacts

- **`DashboardSummary`** should gain a document count for the current user.
- **`Notification`** will be reused for:
  - document shared with a user
  - new document added to a project visible to that user
- **`User.Department`** remains the v1 team boundary for Team Lead rights and department-targeted
  shares.

## Key Relationships and Access Rules

1. A document is always owned by its uploader.
2. A project-linked document is visible to currently entitled project users while the project/task
   context is active.
3. A task-linked document automatically inherits the task's project as its project context.
4. Direct or department/team shares add read-only access but never management rights.
5. Team Lead management uses same-department membership; Project Manager management uses current
   project authority; Administrator bypasses scope restrictions for audit/compliance review.

## Lifecycle / State Transitions

The feature does not require a persisted document status enum for v1; instead, workflow state is
enforced operationally:

1. **Selected** -> user chooses one or more files in the UI
2. **Validated** -> size/type/metadata checks pass
3. **Safety Verified** -> local safety-verification service succeeds
4. **Stored** -> file is saved to local storage
5. **Available** -> metadata is committed and the document appears in authorized views
6. **Updated** -> metadata edit or file replacement completes; document remains available
7. **Shared** -> one or more read-only share grants exist
8. **Deleted** -> file is removed, active shares are revoked, user-facing metadata is removed, and
   only activity history remains available to administrators

## Indexing / Query Notes

- Add indexes for `UploaderUserId`, `ProjectId`, `TaskId`, `Category`, and `UploadedAtUtc` on
  `Document`.
- Add indexes for active share lookups on `DocumentShare`
  (`DocumentId`, `SharedWithUserId`, `SharedWithDepartment`, `RevokedAtUtc`).
- Add indexes for reporting queries on `DocumentActivityRecord`
  (`OccurredAtUtc`, `ActionType`, `ActorUserId`, `ProjectId`).
