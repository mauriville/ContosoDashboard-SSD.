namespace ContosoDashboard.Services;

public class DocumentOptions
{
    public const string SectionName = "DocumentManagement";

    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
    public int MaxFilesPerUpload { get; set; } = 10;
    public string StorageRoot { get; set; } = "AppData/uploads";
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public string[] AllowedMimeTypes { get; set; } = Array.Empty<string>();
    public string[] PreviewableMimeTypes { get; set; } = Array.Empty<string>();
}
