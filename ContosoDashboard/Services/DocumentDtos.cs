namespace ContosoDashboard.Services;

public static class DocumentCategories
{
    public static readonly string[] All =
    [
        "Project Documents",
        "Team Resources",
        "Personal Files",
        "Reports",
        "Presentations",
        "Other"
    ];
}

public class DocumentUploadRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public int? TaskId { get; set; }
    public IReadOnlyCollection<string> Tags { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<DocumentUploadFile> Files { get; set; } = Array.Empty<DocumentUploadFile>();
}

public class DocumentUploadFile
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
    public Func<CancellationToken, Task<Stream>> OpenReadStreamAsync { get; set; } = _ => Task.FromResult<Stream>(Stream.Null);
}

public class UploadDocumentsResponse
{
    public List<UploadDocumentResult> Results { get; set; } = new();
}

public class UploadDocumentResult
{
    public string FileName { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public int? DocumentId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DocumentSummaryDto
{
    public int DocumentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public IReadOnlyList<string> Tags { get; set; } = Array.Empty<string>();
}

public class UploadProjectOptionDto
{
    public int ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
}
