using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoDashboard.Models;

public class DocumentActivityRecord
{
    [Key]
    public int DocumentActivityRecordId { get; set; }

    public int? DocumentId { get; set; }

    [Required]
    public int ActorUserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Outcome { get; set; } = string.Empty;

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public int? ProjectId { get; set; }

    public int? TaskId { get; set; }

    public int? ShareTargetUserId { get; set; }

    [MaxLength(100)]
    public string? ShareTargetDepartment { get; set; }

    [Required]
    [MaxLength(200)]
    public string DocumentTitleSnapshot { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? DocumentCategorySnapshot { get; set; }

    [MaxLength(255)]
    public string? DocumentFileTypeSnapshot { get; set; }

    [MaxLength(1000)]
    public string? ContextSummary { get; set; }

    [ForeignKey(nameof(DocumentId))]
    public virtual Document? Document { get; set; }

    [ForeignKey(nameof(ActorUserId))]
    public virtual User ActorUser { get; set; } = null!;

    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    [ForeignKey(nameof(TaskId))]
    public virtual TaskItem? Task { get; set; }
}
