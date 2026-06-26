using ContosoDashboard.Data;
using ContosoDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentSafetyService _documentSafetyService;
    private readonly DocumentOptions _options;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(
        ApplicationDbContext context,
        IFileStorageService fileStorageService,
        IDocumentSafetyService documentSafetyService,
        IOptions<DocumentOptions> options,
        ILogger<DocumentService> logger)
    {
        _context = context;
        _fileStorageService = fileStorageService;
        _documentSafetyService = documentSafetyService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<UploadDocumentsResponse> UploadDocumentsAsync(int requestingUserId, DocumentUploadRequest request, CancellationToken cancellationToken = default)
    {
        var response = new UploadDocumentsResponse();
        var files = request.Files?.ToList() ?? new List<DocumentUploadFile>();

        if (files.Count == 0)
        {
            response.Results.Add(new UploadDocumentResult
            {
                FileName = "(no file)",
                Outcome = "Rejected",
                Message = "Select at least one file to upload."
            });
            return response;
        }

        if (files.Count > _options.MaxFilesPerUpload)
        {
            foreach (var file in files)
            {
                var message = $"A maximum of {_options.MaxFilesPerUpload} files can be uploaded at one time.";
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = message
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, message, cancellationToken);
            }

            return response;
        }

        var validationError = ValidateSharedMetadata(request);
        var uploadContext = await ResolveUploadContextAsync(requestingUserId, request.ProjectId, request.TaskId, cancellationToken);
        var contextError = uploadContext.ErrorMessage;

        foreach (var file in files)
        {
            if (!string.IsNullOrWhiteSpace(validationError))
            {
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = validationError
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, validationError, cancellationToken);
                continue;
            }

            if (!string.IsNullOrWhiteSpace(contextError))
            {
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = contextError
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, contextError, cancellationToken);
                continue;
            }

            if (file.Length <= 0)
            {
                const string message = "Empty files cannot be uploaded.";
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = message
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, message, cancellationToken);
                continue;
            }

            if (file.Length > _options.MaxFileSizeBytes)
            {
                var message = $"'{file.FileName}' exceeds the 25 MB upload limit.";
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = message
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, message, cancellationToken);
                continue;
            }

            var safetyResult = await _documentSafetyService.VerifyAsync(file, cancellationToken);
            if (!safetyResult.IsSafe)
            {
                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Rejected",
                    Message = safetyResult.Message
                });
                await RecordRejectedUploadAsync(requestingUserId, request, file, safetyResult.Message, cancellationToken);
                continue;
            }

            string? storedPath = null;
            try
            {
                await using var storageStream = await file.OpenReadStreamAsync(cancellationToken);
                storedPath = await _fileStorageService.SaveAsync(
                    storageStream,
                    requestingUserId,
                    uploadContext.ProjectId,
                    safetyResult.NormalizedExtension,
                    cancellationToken);

                var now = DateTime.UtcNow;
                var normalizedTags = NormalizeTags(request.Tags);
                var document = new Document
                {
                    Title = request.Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                    Category = request.Category.Trim(),
                    OriginalFileName = Path.GetFileName(file.FileName),
                    StoredFilePath = storedPath,
                    FileType = safetyResult.NormalizedContentType,
                    FileExtension = safetyResult.NormalizedExtension,
                    FileSizeBytes = file.Length,
                    StorageProvider = "LocalFileSystem",
                    UploaderUserId = requestingUserId,
                    ProjectId = uploadContext.ProjectId,
                    TaskId = uploadContext.TaskId,
                    UploadedAtUtc = now,
                    UpdatedAtUtc = now
                };

                foreach (var tag in normalizedTags)
                {
                    document.Tags.Add(new DocumentTag
                    {
                        TagValue = tag,
                        CreatedAtUtc = now
                    });
                }

                _context.Documents.Add(document);
                _context.DocumentActivityRecords.Add(new DocumentActivityRecord
                {
                    Document = document,
                    ActorUserId = requestingUserId,
                    ActionType = "Upload",
                    Outcome = "Succeeded",
                    OccurredAtUtc = now,
                    ProjectId = uploadContext.ProjectId,
                    TaskId = uploadContext.TaskId,
                    DocumentTitleSnapshot = document.Title,
                    DocumentCategorySnapshot = document.Category,
                    DocumentFileTypeSnapshot = document.FileType,
                    ContextSummary = uploadContext.ContextSummary
                });

                await _context.SaveChangesAsync(cancellationToken);

                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Succeeded",
                    DocumentId = document.DocumentId,
                    Message = "Uploaded successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Document upload failed for user {UserId} and file {FileName}", requestingUserId, file.FileName);

                if (!string.IsNullOrWhiteSpace(storedPath))
                {
                    try
                    {
                        await _fileStorageService.DeleteAsync(storedPath, cancellationToken);
                    }
                    catch (Exception cleanupException)
                    {
                        _logger.LogWarning(cleanupException, "Failed to clean up stored upload '{StoredPath}'", storedPath);
                    }
                }

                response.Results.Add(new UploadDocumentResult
                {
                    FileName = file.FileName,
                    Outcome = "Failed",
                    Message = "The upload could not be completed."
                });
            }
        }

        return response;
    }

    public async Task<IReadOnlyList<DocumentSummaryDto>> GetMyDocumentsAsync(int requestingUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .Include(d => d.Project)
            .Include(d => d.Tags)
            .Where(d => d.UploaderUserId == requestingUserId)
            .OrderByDescending(d => d.UploadedAtUtc)
            .Select(d => new DocumentSummaryDto
            {
                DocumentId = d.DocumentId,
                Title = d.Title,
                Description = d.Description,
                Category = d.Category,
                OriginalFileName = d.OriginalFileName,
                FileType = d.FileType,
                FileSizeBytes = d.FileSizeBytes,
                UploadedAtUtc = d.UploadedAtUtc,
                ProjectId = d.ProjectId,
                ProjectName = d.Project != null ? d.Project.Name : null,
                Tags = d.Tags.OrderBy(t => t.TagValue).Select(t => t.TagValue).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UploadProjectOptionDto>> GetAvailableUploadProjectsAsync(int requestingUserId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == requestingUserId, cancellationToken);

        if (user is null)
        {
            return Array.Empty<UploadProjectOptionDto>();
        }

        var query = _context.Projects
            .AsNoTracking()
            .Where(p => p.Status == ProjectStatus.Active);

        if (user.Role != UserRole.Administrator)
        {
            query = query.Where(p =>
                p.ProjectManagerId == requestingUserId ||
                p.ProjectMembers.Any(pm => pm.UserId == requestingUserId));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new UploadProjectOptionDto
            {
                ProjectId = p.ProjectId,
                Name = p.Name
            })
            .ToListAsync(cancellationToken);
    }

    private string? ValidateSharedMetadata(DocumentUploadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200)
        {
            return "Title is required and must be 200 characters or fewer.";
        }

        if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Trim().Length > 2000)
        {
            return "Description must be 2000 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.Category) ||
            !DocumentCategories.All.Contains(request.Category.Trim(), StringComparer.Ordinal))
        {
            return "Select a valid document category.";
        }

        var invalidTag = request.Tags
            .Select(tag => tag?.Trim() ?? string.Empty)
            .FirstOrDefault(tag => tag.Length > 50);

        return invalidTag is null
            ? null
            : "Tags must be 50 characters or fewer.";
    }

    private async Task RecordRejectedUploadAsync(
        int requestingUserId,
        DocumentUploadRequest request,
        DocumentUploadFile file,
        string message,
        CancellationToken cancellationToken)
    {
        try
        {
            _context.DocumentActivityRecords.Add(new DocumentActivityRecord
            {
                ActorUserId = requestingUserId,
                ActionType = "UploadRejected",
                Outcome = "Rejected",
                OccurredAtUtc = DateTime.UtcNow,
                ProjectId = request.ProjectId,
                TaskId = request.TaskId,
                DocumentTitleSnapshot = string.IsNullOrWhiteSpace(request.Title) ? Path.GetFileName(file.FileName) : request.Title.Trim(),
                DocumentCategorySnapshot = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
                DocumentFileTypeSnapshot = string.IsNullOrWhiteSpace(file.ContentType) ? null : file.ContentType.Trim(),
                ContextSummary = message
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record rejected upload audit for user {UserId} and file {FileName}", requestingUserId, file.FileName);
        }
    }

    private async Task<UploadContext> ResolveUploadContextAsync(int requestingUserId, int? projectId, int? taskId, CancellationToken cancellationToken)
    {
        if (!taskId.HasValue && !projectId.HasValue)
        {
            return new UploadContext();
        }

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == requestingUserId, cancellationToken);

        if (user is null)
        {
            return new UploadContext { ErrorMessage = "The current user could not be resolved." };
        }

        TaskItem? task = null;
        if (taskId.HasValue)
        {
            task = await _context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .ThenInclude(p => p!.ProjectMembers)
                .FirstOrDefaultAsync(t => t.TaskId == taskId.Value, cancellationToken);

            if (task is null || task.ProjectId is null || task.Project is null)
            {
                return new UploadContext { ErrorMessage = "The selected task could not be used for document upload." };
            }

            if (task.Status == Models.TaskStatus.Completed)
            {
                return new UploadContext { ErrorMessage = "Documents can only be uploaded for active tasks." };
            }

            if (projectId.HasValue && task.ProjectId.Value != projectId.Value)
            {
                return new UploadContext { ErrorMessage = "The selected task does not belong to the chosen project." };
            }

            projectId = task.ProjectId;
        }

        Project? project = null;
        if (projectId.HasValue)
        {
            project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.ProjectMembers)
                .FirstOrDefaultAsync(p => p.ProjectId == projectId.Value, cancellationToken);

            if (project is null)
            {
                return new UploadContext { ErrorMessage = "The selected project could not be found." };
            }

            if (project.Status != ProjectStatus.Active)
            {
                return new UploadContext { ErrorMessage = "Documents can only be uploaded to active projects." };
            }

            var isAuthorized = user.Role == UserRole.Administrator ||
                               project.ProjectManagerId == requestingUserId ||
                               project.ProjectMembers.Any(pm => pm.UserId == requestingUserId);

            if (!isAuthorized)
            {
                return new UploadContext { ErrorMessage = "You are not authorized to upload documents to that project." };
            }
        }

        return new UploadContext
        {
            ProjectId = projectId,
            TaskId = taskId,
            ContextSummary = project is null
                ? "Personal upload"
                : $"Project upload: {project.Name}"
        };
    }

    private static List<string> NormalizeTags(IEnumerable<string> tags) =>
        tags
            .Select(tag => tag?.Trim().ToLowerInvariant() ?? string.Empty)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private sealed class UploadContext
    {
        public int? ProjectId { get; init; }
        public int? TaskId { get; init; }
        public string? ContextSummary { get; init; }
        public string? ErrorMessage { get; init; }
    }
}
