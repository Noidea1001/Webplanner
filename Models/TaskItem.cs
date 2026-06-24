using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebPlanner.Models;

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public class TaskItem
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? EndDate { get; set; }

    /// <summary>Comma-separated hashtags, e.g. "#work,#urgent"</summary>
    [StringLength(500)]
    public string? Hashtags { get; set; }

    public bool IsCompleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Owner
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Self-referencing relationship for subtasks
    public int? ParentTaskId { get; set; }
    [ForeignKey(nameof(ParentTaskId))]
    public TaskItem? ParentTask { get; set; }
    public List<TaskItem> SubTasks { get; set; } = new();

    public List<TaskComment> Comments { get; set; } = new();
    public List<TaskAttachment> Attachments { get; set; } = new();

    [NotMapped]
    public bool IsSubtask => ParentTaskId.HasValue;

    [NotMapped]
    public List<string> HashtagList =>
        string.IsNullOrWhiteSpace(Hashtags)
            ? new List<string>()
            : Hashtags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}
