using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebPlanner.Models.ViewModels;

public class TaskFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000)]
    public string? Description { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public string? Hashtags { get; set; }

    public bool IsCompleted { get; set; }

    public int? ParentTaskId { get; set; }

    public IFormFile? NewAttachment { get; set; }

    public List<SelectListItem> AvailableParentTasks { get; set; } = new();
}

public class CommentFormViewModel
{
    public int TaskItemId { get; set; }

    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;
}

public class TaskSearchViewModel
{
    public string? Query { get; set; }
    public TaskPriority? Priority { get; set; }
    public string? Hashtag { get; set; }
    public List<TaskItem> Results { get; set; } = new();
}

public enum CalendarRange
{
    Day,
    Week,
    Month,
    Year
}

public class CalendarViewModel
{
    public CalendarRange Range { get; set; } = CalendarRange.Week;
    public DateTime AnchorDate { get; set; } = DateTime.Today;
    public DateTime RangeStart { get; set; }
    public DateTime RangeEnd { get; set; }
    public List<TaskItem> Tasks { get; set; } = new();
}

public class DashboardViewModel
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double CompletionRate { get; set; }
    public int LowPriorityCount { get; set; }
    public int MediumPriorityCount { get; set; }
    public int HighPriorityCount { get; set; }
    public List<KeyValuePair<string, int>> PopularTags { get; set; } = new();
    public List<TaskItem> UpcomingTasks { get; set; } = new();
    public List<string> WeekDayLabels { get; set; } = new();
    public List<int> TasksDueByDay { get; set; } = new();
    public List<int> TasksCreatedByDay { get; set; } = new();
    public List<DashboardCalendarDay> CalendarDays { get; set; } = new();
    public string CalendarMonthLabel { get; set; } = string.Empty;
}

public class DashboardCalendarDay
{
    public DateTime Date { get; set; }
    public int TaskCount { get; set; }
    public bool IsToday { get; set; }
    public bool IsCurrentMonth { get; set; }
}
