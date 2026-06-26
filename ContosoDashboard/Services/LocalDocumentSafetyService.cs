using Microsoft.Extensions.Options;

namespace ContosoDashboard.Services;

public class LocalDocumentSafetyService : IDocumentSafetyService
{
    private static readonly byte[] PdfSignature = [0x25, 0x50, 0x44, 0x46, 0x2D];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] ZipSignature = [0x50, 0x4B];
    private static readonly byte[] OleSignature = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];

    private readonly HashSet<string> _allowedExtensions;
    private readonly HashSet<string> _allowedMimeTypes;

    private static readonly Dictionary<string, string[]> MimeTypesByExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = ["application/pdf"],
        [".doc"] = ["application/msword"],
        [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
        [".xls"] = ["application/vnd.ms-excel"],
        [".xlsx"] = ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
        [".ppt"] = ["application/vnd.ms-powerpoint"],
        [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
        [".txt"] = ["text/plain"],
        [".jpg"] = ["image/jpeg"],
        [".jpeg"] = ["image/jpeg"],
        [".png"] = ["image/png"]
    };

    public LocalDocumentSafetyService(IOptions<DocumentOptions> options)
    {
        _allowedExtensions = options.Value.AllowedExtensions
            .Select(NormalizeExtension)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _allowedMimeTypes = options.Value.AllowedMimeTypes
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public async Task<DocumentSafetyVerificationResult> VerifyAsync(DocumentUploadFile file, CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return Reject("No file was supplied.");
        }

        var normalizedExtension = NormalizeExtension(Path.GetExtension(file.FileName));
        if (!_allowedExtensions.Contains(normalizedExtension))
        {
            return Reject($"Files with the '{normalizedExtension}' extension are not supported.");
        }

        var normalizedContentType = NormalizeContentType(file.ContentType);
        if (!MimeTypesByExtension.TryGetValue(normalizedExtension, out var allowedTypesForExtension))
        {
            return Reject("The file type is not supported.");
        }

        if (!string.IsNullOrWhiteSpace(normalizedContentType) &&
            !normalizedContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase) &&
            (!_allowedMimeTypes.Contains(normalizedContentType) ||
             !allowedTypesForExtension.Contains(normalizedContentType, StringComparer.OrdinalIgnoreCase)))
        {
            return Reject("The uploaded content type does not match the supported file type.");
        }

        try
        {
            await using var stream = await file.OpenReadStreamAsync(cancellationToken);
            if (stream is null || !stream.CanRead)
            {
                return Reject("The uploaded file could not be read for safety verification.");
            }

            var header = new byte[16];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
            if (bytesRead <= 0)
            {
                return Reject("The uploaded file is empty or unreadable.");
            }

            if (!MatchesExpectedSignature(normalizedExtension, header, bytesRead, stream))
            {
                return Reject("The uploaded file failed training safety verification.");
            }

            var detectedContentType = normalizedContentType;
            if (string.IsNullOrWhiteSpace(detectedContentType) || detectedContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
            {
                detectedContentType = allowedTypesForExtension[0];
            }

            return new DocumentSafetyVerificationResult
            {
                IsSafe = true,
                Message = "Training safety verification passed.",
                NormalizedExtension = normalizedExtension,
                NormalizedContentType = detectedContentType
            };
        }
        catch
        {
            return Reject("Safety verification could not be completed. The upload was rejected.");
        }
    }

    private static bool MatchesExpectedSignature(string extension, byte[] header, int bytesRead, Stream stream)
    {
        return extension switch
        {
            ".pdf" => StartsWith(header, bytesRead, PdfSignature),
            ".png" => StartsWith(header, bytesRead, PngSignature),
            ".jpg" or ".jpeg" => StartsWith(header, bytesRead, JpegSignature),
            ".docx" or ".xlsx" or ".pptx" => StartsWith(header, bytesRead, ZipSignature),
            ".doc" or ".xls" or ".ppt" => StartsWith(header, bytesRead, OleSignature),
            ".txt" => IsTextLike(header, bytesRead, stream),
            _ => false
        };
    }

    private static bool IsTextLike(byte[] header, int bytesRead, Stream stream)
    {
        if (ContainsNullByte(header, bytesRead))
        {
            return false;
        }

        Span<byte> buffer = stackalloc byte[256];
        var additionalRead = stream.Read(buffer);
        for (var i = 0; i < additionalRead; i++)
        {
            if (buffer[i] == 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ContainsNullByte(byte[] buffer, int count)
    {
        for (var i = 0; i < count; i++)
        {
            if (buffer[i] == 0)
            {
                return true;
            }
        }

        return false;
    }

    private static bool StartsWith(byte[] source, int sourceLength, byte[] signature)
    {
        if (sourceLength < signature.Length)
        {
            return false;
        }

        for (var i = 0; i < signature.Length; i++)
        {
            if (source[i] != signature[i])
            {
                return false;
            }
        }

        return true;
    }

    private static DocumentSafetyVerificationResult Reject(string message) =>
        new()
        {
            IsSafe = false,
            Message = message
        };

    private static string NormalizeExtension(string? extension)
    {
        var value = string.IsNullOrWhiteSpace(extension) ? string.Empty : extension.Trim();
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.StartsWith('.') ? value.ToLowerInvariant() : $".{value.ToLowerInvariant()}";
    }

    private static string NormalizeContentType(string? contentType) =>
        string.IsNullOrWhiteSpace(contentType)
            ? string.Empty
            : contentType.Split(';', 2)[0].Trim();
}
