using System.Security.Claims;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Endpoints;

public static class DocumentApiEndpoints
{
    public static IEndpointRouteBuilder MapDocumentApiEndpoints(this IEndpointRouteBuilder app)
    {
        var documents = app.MapGroup("/api/documents")
            .RequireAuthorization();

        documents.MapPost("/", UploadDocumentsAsync)
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> UploadDocumentsAsync(
        HttpContext httpContext,
        IDocumentService documentService,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdClaim, out var requestingUserId))
        {
            return Results.Unauthorized();
        }

        if (!httpContext.Request.HasFormContentType)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Invalid request",
                Detail = "Use multipart/form-data when uploading documents."
            });
        }

        var form = await httpContext.Request.ReadFormAsync(cancellationToken);
        var files = form.Files.Select(file => new DocumentUploadFile
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            OpenReadStreamAsync = _ => Task.FromResult<Stream>(file.OpenReadStream())
        }).ToList();

        var tags = form.TryGetValue("tags", out var tagValues)
            ? tagValues.SelectMany(value => value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList()
            : new List<string>();

        var request = new DocumentUploadRequest
        {
            Title = form["title"].ToString(),
            Description = form["description"].ToString(),
            Category = form["category"].ToString(),
            ProjectId = int.TryParse(form["projectId"], out var projectId) ? projectId : null,
            TaskId = int.TryParse(form["taskId"], out var taskId) ? taskId : null,
            Tags = tags,
            Files = files
        };

        var response = await documentService.UploadDocumentsAsync(requestingUserId, request, cancellationToken);
        var hasSuccess = response.Results.Any(result => result.Outcome == "Succeeded");

        return hasSuccess
            ? Results.Created("/api/documents", response)
            : Results.BadRequest(response);
    }
}
