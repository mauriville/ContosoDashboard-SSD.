# Quickstart: Document Upload and Management

## Purpose

This quickstart describes how to validate the document upload and management feature locally in the
training environment once implementation is complete.

## Prerequisites

- .NET 8 SDK installed
- Repository checked out at
  `/Users/mauriciovillegas/development/coderoad/ContosoDashboard-SSD.`
- Local write access for SQLite and the upload folder under `ContosoDashboard/AppData/uploads`

## Local Setup

1. Start from the repository root:

   ```bash
   cd /Users/mauriciovillegas/development/coderoad/ContosoDashboard-SSD.
   ```

2. Run the app:

   ```bash
   cd ContosoDashboard
   dotnet run
   ```

3. Open the application in the browser and sign in with one of the seeded training users:

   - System Administrator
   - Camille Nicole (Project Manager)
   - Floris Kregel (Team Lead)
   - Ni Kang (Employee)

## Resetting Local State

Use a clean state before verification when testing replacement, delete, or failed-upload scenarios.

- Remove the SQLite database file: `ContosoDashboard/ContosoDashboard.db`
- Remove the local upload folder: `ContosoDashboard/AppData/uploads`
- Restart the app so `EnsureCreated()` reseeds the training data

## Verification Scenario 1: Upload and Categorize

1. Sign in as **Ni Kang**.
2. Open the new document upload flow from My Documents or a task/project context.
3. Upload a supported file <= 25 MB with title and category.
4. Confirm:
   - per-file progress/result is shown
   - the document appears in My Documents
   - metadata shows title, category, upload date, size, type, uploader, and project if set
5. Repeat with:
   - an unsupported extension
   - a file > 25 MB
   - a file that fails safety verification
6. Confirm each failed upload is rejected before it becomes visible.

## Verification Scenario 2: Browse, Search, Preview, and Download

1. Upload at least one personal document and one project-linked document.
2. Confirm My Documents supports sort/filter by title, upload date, category, size, project, and
   date range.
3. Search by title, description, tag, uploader, and project name.
4. Confirm only authorized documents appear in results.
5. Preview a PDF or image inline.
6. Download an authorized document and confirm unauthorized users cannot preview or download it.

## Verification Scenario 3: Share and Management Rules

1. As **Ni Kang**, share a document with **Floris Kregel** and confirm:
   - Floris receives an in-app notification
   - the document appears in Shared with Me
   - Floris can preview/download but cannot edit, replace, delete, or reshare
2. As **Floris Kregel** (Team Lead), verify management rights over same-department team documents.
3. As **Camille Nicole** (Project Manager), verify management rights for documents linked to her
   project.
4. As **System Administrator**, verify full access.
5. Delete a document and confirm:
   - confirmation is required
   - the file is no longer previewable/downloadable
   - active shares are revoked
   - activity history remains available to administrators

## Verification Scenario 4: Project, Task, Dashboard, and Notification Integration

1. Upload a document from a task context and confirm it is associated with the task's project.
2. Open project details and confirm related documents appear only for authorized project members.
3. Confirm inactive project/task contexts no longer surface the document while other valid
   entitlements still work.
4. Open the dashboard and confirm:
   - recent documents widget shows the five latest user uploads
   - summary cards include a document count
5. Confirm project members receive notifications when a new document is added to one of their
   projects.

## Verification Scenario 5: Audit and Reporting

1. Perform upload, metadata update, share, download, replace, and delete actions.
2. Sign in as **System Administrator**.
3. Open the document activity/reporting experience.
4. Confirm audit entries capture actor, action, document, timestamp, outcome, and project/share
   context.
5. Confirm reports cover:
   - most uploaded document types
   - most active uploaders
   - document access patterns

## Notes / Assumptions

- The feature remains training-only; local safety verification is not a claim of production-grade
  malware scanning.
- Team-based access in v1 maps to the repository's existing `User.Department` concept.
- Authorized file access must flow through the application, not through static file URLs.
