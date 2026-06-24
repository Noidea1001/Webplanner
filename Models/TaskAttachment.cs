namespace WebPlanner.Models;

public class TaskAttachment
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }

    public string FileName { get; set; } = string.Empty;

    /// <summary>Relative path under wwwroot/uploads</summary>
    public string FilePath { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
