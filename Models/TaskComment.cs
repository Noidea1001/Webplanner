using System.ComponentModel.DataAnnotations;

namespace WebPlanner.Models;

public class TaskComment
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string AuthorUserId { get; set; } = string.Empty;
}
