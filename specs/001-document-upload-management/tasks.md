# Tasks: Document Upload and Management

**Input**: Design documents from `/Users/mauriciovillegas/development/coderoad/ContosoDashboard-SSD./specs/001-document-upload-management/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/document-management.openapi.yaml`

**Tests**: Use explicit manual verification tasks in `specs/001-document-upload-management/quickstart.md`; do not add a new test project unless implementation uncovers a high-risk gap that cannot be verified manually.

**Organization**: Tasks are grouped by user story so each increment can be implemented and validated independently in the existing `ContosoDashboard/` Blazor Server application.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Task can run in parallel with other marked tasks because it touches different files and has no dependency on incomplete work in the same phase.
- **[Story]**: User story label for traceability (`[US1]`, `[US2]`, `[US3]`, `[US4]`).
- Every task includes the concrete repository file path that should be changed or verified.

## Path Conventions

- **Application project**: `ContosoDashboard/`
- **Data access**: `ContosoDashboard/Data/`
- **Domain and view models**: `ContosoDashboard/Models/`
- **Business and infrastructure services**: `ContosoDashboard/Services/`
- **UI pages and shared components**: `ContosoDashboard/Pages/` and `ContosoDashboard/Shared/`
- **Feature documentation**: `specs/001-document-upload-management/`, `README.md`, and `StakeholderDocs/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Confirm repository constraints, reserve local storage conventions, and prepare the app for document-management wiring.

- [ ] T001 Review document constraints and offline/security expectations in `README.md`, `StakeholderDocs/document-upload-and-management-feature.md`, and `specs/001-document-upload-management/plan.md`
- [ ] T002 Add document-management configuration for upload limits, storage root, and previewable MIME types in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [ ] T003 [P] Reserve local upload storage and ignore generated binaries in `.gitignore` and `ContosoDashboard/AppData/uploads/.gitkeep`
- [ ] T004 [P] Create the document endpoint scaffold and initial feature registration points in `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs` and `ContosoDashboard/Program.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build the shared persistence, storage, safety, and authorization foundation required before any user-story workflow can ship.

**⚠️ CRITICAL**: Complete this phase before starting user-story implementation.

- [ ] T005 [P] Add document domain entities in `ContosoDashboard/Models/Document.cs`, `ContosoDashboard/Models/DocumentTag.cs`, `ContosoDashboard/Models/DocumentShare.cs`, and `ContosoDashboard/Models/DocumentActivityRecord.cs`
- [ ] T006 [P] Extend existing navigation and notification enums for document workflows in `ContosoDashboard/Models/User.cs`, `ContosoDashboard/Models/Project.cs`, `ContosoDashboard/Models/TaskItem.cs`, and `ContosoDashboard/Models/Notification.cs`
- [ ] T007 Update SQLite mappings, indexes, and seed-safe document configuration in `ContosoDashboard/Data/ApplicationDbContext.cs`
- [ ] T008 [P] Implement local file-storage abstractions in `ContosoDashboard/Services/IFileStorageService.cs` and `ContosoDashboard/Services/LocalFileStorageService.cs`
- [ ] T009 [P] Implement offline safety-verification abstractions in `ContosoDashboard/Services/IDocumentSafetyService.cs` and `ContosoDashboard/Services/LocalDocumentSafetyService.cs`
- [ ] T010 Implement shared document DTOs, authorization helpers, and base service contracts in `ContosoDashboard/Services/IDocumentService.cs`, `ContosoDashboard/Services/DocumentService.cs`, and `ContosoDashboard/Services/DocumentDtos.cs`
- [ ] T011 Update dependency injection, upload request limits, and document endpoint mapping in `ContosoDashboard/Program.cs` and `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs`
- [ ] T012 Capture SQLite and upload-folder setup/reset guidance in `README.md` and `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: Document persistence, local storage, fail-closed safety checks, and DI wiring are ready for user-story work.

---

## Phase 3: User Story 1 - Upload and categorize documents (Priority: P1) 🎯 MVP

**Goal**: Let authenticated users upload supported files with required metadata into personal or active project context using local storage and fail-closed safety verification.

**Independent Test**: Sign in as an authorized employee, upload supported files with title/category metadata, confirm successful files appear in My Documents with captured details, and confirm oversize, unsupported, or safety-failed files never become available.

### Verification for User Story 1

- [ ] T013 [P] [US1] Add upload request and per-file result models for the contract in `ContosoDashboard/Services/DocumentDtos.cs` and `ContosoDashboard/Services/IDocumentService.cs`
- [ ] T014 [P] [US1] Build reusable upload form and progress UI in `ContosoDashboard/Shared/DocumentUploadPanel.razor` and `ContosoDashboard/Shared/DocumentUploadResults.razor`
- [ ] T015 [US1] Implement upload authorization, validation, safety verification, local file persistence, and rejected-upload auditing in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T016 [US1] Implement POST `/api/documents` with contract-aligned success and failure responses in `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs`
- [ ] T017 [US1] Create the My Documents upload workflow with title/category/project/tag inputs in `ContosoDashboard/Pages/Documents.razor` and wire entry navigation in `ContosoDashboard/Shared/NavMenu.razor`
- [ ] T018 [US1] Run and record supported-upload, oversize, unsupported-type, and safety-failure checks in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: User Story 1 delivers the MVP upload experience with local storage, required metadata, and fail-closed document safety verification.

---

## Phase 4: User Story 2 - Find and access authorized documents (Priority: P1)

**Goal**: Let users browse, filter, search, preview, and download only the documents they are authorized to access across personal, shared, project, task, and dashboard contexts.

**Independent Test**: Upload personal and project-linked documents, sign in as multiple roles, verify list/search results honor ownership and current entitlements, preview PDFs/images inline, and deny unauthorized preview/download requests without exposing record details.

### Verification for User Story 2

- [ ] T019 [P] [US2] Add authorized list, search, filter, and preview projection models in `ContosoDashboard/Services/DocumentDtos.cs` and `ContosoDashboard/Services/IDocumentService.cs`
- [ ] T020 [P] [US2] Build reusable filter, results-table, and preview components in `ContosoDashboard/Shared/DocumentFilterBar.razor`, `ContosoDashboard/Shared/DocumentTable.razor`, and `ContosoDashboard/Shared/DocumentPreviewModal.razor`
- [ ] T021 [US2] Implement browse, detail, shared, project, task, preview, and download logic with fail-closed access checks in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T022 [US2] Implement GET `/api/documents`, `/api/documents/{documentId}`, `/api/documents/{documentId}/content`, `/api/documents/shared-with-me`, `/api/projects/{projectId}/documents`, and `/api/tasks/{taskId}/documents` in `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs`
- [ ] T023 [US2] Expand `ContosoDashboard/Pages/Documents.razor` for My Documents, Shared with Me, sort/filter/search, preview, and download actions
- [ ] T024 [US2] Create task-specific document access in `ContosoDashboard/Pages/TaskDetails.razor` and link to it from `ContosoDashboard/Pages/Tasks.razor`
- [ ] T025 [US2] Add project-related document browsing and access affordances in `ContosoDashboard/Pages/ProjectDetails.razor`
- [ ] T026 [US2] Add document counts and recent-document widgets to `ContosoDashboard/Services/DashboardService.cs`, `ContosoDashboard/Services/DocumentService.cs`, and `ContosoDashboard/Pages/Index.razor`
- [ ] T027 [US2] Run and record cross-user browse, search, preview, download, and inactive-context checks in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: User Story 2 delivers authorized discovery and access across dashboard, project, task, and shared-document experiences.

---

## Phase 5: User Story 3 - Manage and share documents in context (Priority: P2)

**Goal**: Let owners and authorized managers update metadata, replace files, delete documents, and create read-only shares while notifying recipients and preserving service-level authorization boundaries.

**Independent Test**: Update a document’s metadata, replace its file, delete it with confirmation, share it to a user or department, verify recipients receive notifications and read-only access, and verify share recipients cannot edit, replace, delete, or reshare.

### Verification for User Story 3

- [ ] T028 [P] [US3] Add metadata-update, replacement, and share request models in `ContosoDashboard/Services/DocumentDtos.cs` and `ContosoDashboard/Services/IDocumentService.cs`
- [ ] T029 [P] [US3] Build edit, share, and delete UI components in `ContosoDashboard/Shared/DocumentMetadataEditor.razor`, `ContosoDashboard/Shared/DocumentSharePanel.razor`, and `ContosoDashboard/Shared/DocumentDeleteDialog.razor`
- [ ] T030 [US3] Implement metadata edit, replacement, delete, share, and share-revocation workflows with owner, team, project-manager, and admin rules in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T031 [US3] Implement PUT `/api/documents/{documentId}`, DELETE `/api/documents/{documentId}`, POST `/api/documents/{documentId}/replacement`, and POST `/api/documents/{documentId}/shares` in `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs`
- [ ] T032 [US3] Wire edit, replace, delete, and share actions into `ContosoDashboard/Pages/Documents.razor`, `ContosoDashboard/Pages/ProjectDetails.razor`, and `ContosoDashboard/Pages/TaskDetails.razor`
- [ ] T033 [US3] Extend document-share and project-document notifications in `ContosoDashboard/Models/Notification.cs`, `ContosoDashboard/Services/NotificationService.cs`, and `ContosoDashboard/Services/DocumentService.cs`
- [ ] T034 [US3] Update `ContosoDashboard/Pages/Notifications.razor` and `ContosoDashboard/Pages/Index.razor` to surface document-share and project-document alerts with navigation links
- [ ] T035 [US3] Run and record owner, team-lead, project-manager, administrator, and share-recipient management-rule verification in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: User Story 3 delivers secure document management and read-only sharing without weakening authorization boundaries.

---

## Phase 6: User Story 4 - Review document activity and usage (Priority: P3)

**Goal**: Let administrators review durable audit history and usage reports for document adoption, access, and deleted-document activity.

**Independent Test**: Perform upload, download, replace, share, and delete actions, sign in as the administrator, confirm all required audit fields are preserved, and verify non-admin users cannot access reporting endpoints or UI.

### Verification for User Story 4

- [ ] T036 [P] [US4] Add audit-history and usage-report DTOs in `ContosoDashboard/Services/DocumentDtos.cs` and `ContosoDashboard/Services/IDocumentService.cs`
- [ ] T037 [US4] Implement document activity persistence, denied-access logging, and aggregate usage queries in `ContosoDashboard/Services/DocumentService.cs`
- [ ] T038 [US4] Implement GET `/api/admin/document-activity` and GET `/api/admin/document-reports/usage` with administrator-only enforcement in `ContosoDashboard/Endpoints/DocumentApiEndpoints.cs`
- [ ] T039 [US4] Create the administrator audit/reporting page in `ContosoDashboard/Pages/DocumentReports.razor` and add admin navigation in `ContosoDashboard/Shared/NavMenu.razor`
- [ ] T040 [US4] Add report filters, deleted-document history display, and export-ready summary tables in `ContosoDashboard/Pages/DocumentReports.razor` and `ContosoDashboard/wwwroot/css/site.css`
- [ ] T041 [US4] Run and record administrator-only audit/report access checks plus deleted-document history validation in `specs/001-document-upload-management/quickstart.md`

**Checkpoint**: User Story 4 delivers administrator-only audit and reporting coverage for the full document lifecycle.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Finish training-specific documentation, UX refinement, and end-to-end verification across all stories.

- [ ] T042 [P] Update training-only documentation and migration notes for local file storage and safety verification in `README.md`, `StakeholderDocs/document-upload-and-management-feature.md`, and `specs/001-document-upload-management/research.md`
- [ ] T043 [P] Refine document navigation, responsive styling, and preview accessibility in `ContosoDashboard/Shared/NavMenu.razor`, `ContosoDashboard/Shared/MainLayout.razor`, and `ContosoDashboard/wwwroot/css/site.css`
- [ ] T044 Verify offline happy-path, unauthorized-path, and reset-guidance consistency in `specs/001-document-upload-management/quickstart.md` and `specs/001-document-upload-management/tasks.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup** → no dependencies
- **Phase 2: Foundational** → depends on Phase 1 and blocks all user stories
- **Phase 3: US1** → depends on Phase 2
- **Phase 4: US2** → depends on Phase 2 and should start after the upload pipeline in US1 is usable
- **Phase 5: US3** → depends on US1 and US2 because management and sharing build on stored and discoverable documents
- **Phase 6: US4** → depends on US1, US2, and US3 because reporting needs the full action history
- **Phase 7: Polish** → depends on all targeted user stories being complete

### User Story Dependencies

- **US1 (P1)**: First MVP increment; no dependency on later stories
- **US2 (P1)**: Requires the foundational document model plus a working upload path to supply accessible documents
- **US3 (P2)**: Requires existing browse/detail flows so managers can act on current documents and shares
- **US4 (P3)**: Requires upload, access, sharing, replacement, and delete actions to generate meaningful audit/report data

### Within Each User Story

- Add or extend DTOs/contracts before service implementation
- Complete service-layer authorization and business logic before wiring endpoints or UI
- Wire page and component integrations after service methods are available
- Finish with explicit manual verification notes in `specs/001-document-upload-management/quickstart.md`

### Parallel Opportunities

- `T003` and `T004` can run in parallel after configuration planning starts
- `T005`, `T006`, `T008`, and `T009` can run in parallel in the foundational phase
- In US1, `T013` and `T014` can run in parallel before `T015`
- In US2, `T019` and `T020` can run in parallel before `T021`, and `T024` and `T025` can proceed independently after `T022`
- In US3, `T028` and `T029` can run in parallel before `T030`, and `T033` can proceed alongside `T032` once management flows exist
- In US4, `T036` can run in parallel with the initial admin page shell in `T039` after the data shape is agreed

---

## Parallel Example: User Story 1

```bash
# Parallel prep for US1
Task T013: Add upload request and result models in ContosoDashboard/Services/DocumentDtos.cs and ContosoDashboard/Services/IDocumentService.cs
Task T014: Build upload form and progress UI in ContosoDashboard/Shared/DocumentUploadPanel.razor and ContosoDashboard/Shared/DocumentUploadResults.razor
```

## Parallel Example: User Story 2

```bash
# Parallel prep for US2
Task T019: Add authorized list/search models in ContosoDashboard/Services/DocumentDtos.cs and ContosoDashboard/Services/IDocumentService.cs
Task T020: Build filter, table, and preview components in ContosoDashboard/Shared/DocumentFilterBar.razor, ContosoDashboard/Shared/DocumentTable.razor, and ContosoDashboard/Shared/DocumentPreviewModal.razor

# Parallel integrations after service + endpoints are ready
Task T024: Create task-specific document access in ContosoDashboard/Pages/TaskDetails.razor and ContosoDashboard/Pages/Tasks.razor
Task T025: Add project-related document browsing in ContosoDashboard/Pages/ProjectDetails.razor
```

## Parallel Example: User Story 3

```bash
# Parallel prep for US3
Task T028: Add metadata-update, replacement, and share request models in ContosoDashboard/Services/DocumentDtos.cs and ContosoDashboard/Services/IDocumentService.cs
Task T029: Build edit, share, and delete UI components in ContosoDashboard/Shared/DocumentMetadataEditor.razor, ContosoDashboard/Shared/DocumentSharePanel.razor, and ContosoDashboard/Shared/DocumentDeleteDialog.razor
```

## Parallel Example: User Story 4

```bash
# Parallel prep for US4
Task T036: Add audit and usage report DTOs in ContosoDashboard/Services/DocumentDtos.cs and ContosoDashboard/Services/IDocumentService.cs
Task T039: Create the admin reporting shell in ContosoDashboard/Pages/DocumentReports.razor and ContosoDashboard/Shared/NavMenu.razor
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. Validate upload success, rejection, and safety-failure behavior with `specs/001-document-upload-management/quickstart.md`
5. Demo the MVP before moving to discovery, sharing, or reporting work

### Incremental Delivery

1. Finish Setup + Foundational to establish SQLite, storage, safety, and DI seams
2. Ship **US1** for local upload and categorization
3. Add **US2** for browse/search/preview/download across dashboard, project, and task contexts
4. Add **US3** for metadata management, file replacement, delete, shares, and notifications
5. Add **US4** for administrator audit history and usage reports
6. Close with documentation and end-to-end offline verification in Phase 7

### Parallel Team Strategy

1. One developer handles persistence and storage (`T005`-`T009`) while another prepares API/page scaffolding (`T004`, `T011`)
2. After the foundation is complete, one developer can drive `DocumentService`/endpoint work while another builds shared Blazor components for the same story
3. Once US2 is ready, project/task page integration and dashboard integration can be split across separate contributors

---

## Notes

- The plan intentionally favors explicit manual verification tasks over creating a new automated test project.
- All file access must stay outside `wwwroot` and pass through authorized application flows.
- Every service and endpoint task must preserve fail-closed authorization for owner, department/team, project, share, and administrator paths.
- If implementation reveals a missing test harness is necessary for a high-risk regression, add it in the smallest possible scope and update this file before coding further.
