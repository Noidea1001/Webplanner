using System.ComponentModel.DataAnnotations;

namespace WebPlanner.Models.Dtos;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Hashtags { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ParentTaskId { get; set; }
    public List<int> SubTaskIds { get; set; } = new();
    public List<CommentDto> Comments { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();

    public static TaskDto FromEntity(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        Priority = t.Priority,
        EndDate = t.EndDate,
        Hashtags = t.Hashtags,
        IsCompleted = t.IsCompleted,
        CreatedAt = t.CreatedAt,
        ParentTaskId = t.ParentTaskId,
        SubTaskIds = t.SubTasks?.Select(s => s.Id).ToList() ?? new(),
        Comments = t.Comments?.Select(CommentDto.FromEntity).ToList() ?? new(),
        Attachments = t.Attachments?.Select(AttachmentDto.FromEntity).ToList() ?? new()
    };
}

public class CommentDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static CommentDto FromEntity(TaskComment c) => new()
    {
        Id = c.Id,
        Text = c.Text,
        CreatedAt = c.CreatedAt
    };
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }

    public static AttachmentDto FromEntity(TaskAttachment a) => new()
    {
        Id = a.Id,
        FileName = a.FileName,
        Url = "/uploads/" + a.FilePath,
        FileSizeBytes = a.FileSizeBytes
    };
}

public class TaskCreateUpdateDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? EndDate { get; set; }
    public string? Hashtags { get; set; }
    public bool IsCompleted { get; set; }
    public int? ParentTaskId { get; set; }
}

public class CommentCreateDto
{
    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;
}

public class TaskMoveDto
{
    public TaskPriority Priority { get; set; }
    public bool IsCompleted { get; set; }
}
