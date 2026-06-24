using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;
using WebPlanner.Models.ViewModels;

namespace WebPlanner.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public TasksController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    // GET: /Tasks
    public async Task<IActionResult> Index()
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.ParentTaskId == null)
            .Include(t => t.SubTasks)
            .Include(t => t.Attachments)
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.EndDate)
            .ToListAsync();

        return View(tasks);
    }

    // GET: /Tasks/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .Include(t => t.ParentTask)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (task == null) return NotFound();
        return View(task);
    }

    // GET: /Tasks/Create
    public async Task<IActionResult> Create(int? parentTaskId, TaskPriority? priority, bool? isCompleted)
    {
        var vm = new TaskFormViewModel
        {
            ParentTaskId = parentTaskId,
            Priority = priority ?? TaskPriority.Medium,
            IsCompleted = isCompleted ?? false,
            AvailableParentTasks = await GetParentOptionsAsync(null)
        };
        return View(vm);
    }

    // POST: /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AvailableParentTasks = await GetParentOptionsAsync(null);
            return View(vm);
        }

        var task = new TaskItem
        {
            Title = vm.Title,
            Description = vm.Description,
            Priority = vm.Priority,
            EndDate = vm.EndDate,
            Hashtags = vm.Hashtags,
            IsCompleted = vm.IsCompleted,
            ParentTaskId = vm.ParentTaskId,
            UserId = CurrentUserId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        if (vm.NewAttachment != null)
            await SaveAttachmentAsync(task.Id, vm.NewAttachment);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        var vm = new TaskFormViewModel
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            EndDate = task.EndDate,
            Hashtags = task.Hashtags,
            IsCompleted = task.IsCompleted,
            ParentTaskId = task.ParentTaskId,
            AvailableParentTasks = await GetParentOptionsAsync(task.Id)
        };
        return View(vm);
    }

    // POST: /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskFormViewModel vm)
    {
        if (id != vm.Id) return BadRequest();

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        if (!ModelState.IsValid)
        {
            vm.AvailableParentTasks = await GetParentOptionsAsync(task.Id);
            return View(vm);
        }

        // Prevent setting a task as its own parent
        if (vm.ParentTaskId == task.Id) vm.ParentTaskId = null;

        task.Title = vm.Title;
        task.Description = vm.Description;
        task.Priority = vm.Priority;
        task.EndDate = vm.EndDate;
        task.Hashtags = vm.Hashtags;
        task.IsCompleted = vm.IsCompleted;
        task.ParentTaskId = vm.ParentTaskId;

        await _db.SaveChangesAsync();

        if (vm.NewAttachment != null)
            await SaveAttachmentAsync(task.Id, vm.NewAttachment);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Tasks/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();
        return View(task);
    }

    // POST: /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        // Detach subtasks (don't cascade-delete them)
        foreach (var sub in task.SubTasks)
            sub.ParentTaskId = null;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // POST: /Tasks/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(CommentFormViewModel vm)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == vm.TaskItemId && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(vm.Text))
        {
            _db.Comments.Add(new TaskComment
            {
                TaskItemId = task.Id,
                Text = vm.Text,
                AuthorUserId = CurrentUserId
            });
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = vm.TaskItemId });
    }

    // POST: /Tasks/DeleteAttachment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, int taskId)
    {
        var attachment = await _db.Attachments
            .Include(a => a.TaskItem)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskItem!.UserId == CurrentUserId);

        if (attachment != null)
        {
            var fullPath = Path.Combine(_env.WebRootPath, "uploads", attachment.FilePath);
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

            _db.Attachments.Remove(attachment);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = taskId });
    }

    // GET: /Tasks/Search
    public async Task<IActionResult> Search(TaskSearchViewModel vm)
    {
        var query = _db.Tasks.Where(t => t.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(vm.Query))
        {
            var q = vm.Query.Trim();
            query = query.Where(t => t.Title.Contains(q) || (t.Description != null && t.Description.Contains(q)));
        }

        if (vm.Priority.HasValue)
            query = query.Where(t => t.Priority == vm.Priority.Value);

        if (!string.IsNullOrWhiteSpace(vm.Hashtag))
        {
            var tag = vm.Hashtag.Trim().TrimStart('#');
            query = query.Where(t => t.Hashtags != null && t.Hashtags.Contains(tag));
        }

        vm.Results = await query
            .Include(t => t.Attachments)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        return View(vm);
    }

    // GET: /Tasks/Calendar?range=Week&date=2026-06-22
    public async Task<IActionResult> Calendar(CalendarRange range = CalendarRange.Week, DateTime? date = null)
    {
        var anchor = (date ?? DateTime.Today).Date;
        DateTime start, end;

        switch (range)
        {
            case CalendarRange.Day:
                start = anchor;
                end = anchor.AddDays(1).AddTicks(-1);
                break;
            case CalendarRange.Month:
                start = new DateTime(anchor.Year, anchor.Month, 1);
                end = start.AddMonths(1).AddTicks(-1);
                break;
            case CalendarRange.Year:
                start = new DateTime(anchor.Year, 1, 1);
                end = start.AddYears(1).AddTicks(-1);
                break;
            case CalendarRange.Week:
            default:
                int diff = (7 + (anchor.DayOfWeek - DayOfWeek.Monday)) % 7;
                start = anchor.AddDays(-diff);
                end = start.AddDays(7).AddTicks(-1);
                break;
        }

        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.EndDate != null && t.EndDate >= start && t.EndDate <= end)
            .Include(t => t.Attachments)
            .OrderBy(t => t.EndDate)
            .ToListAsync();

        var vm = new CalendarViewModel
        {
            Range = range,
            AnchorDate = anchor,
            RangeStart = start,
            RangeEnd = end,
            Tasks = tasks
        };

        return View(vm);
    }

    // GET: /Tasks/Board
    public async Task<IActionResult> Board()
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.ParentTaskId == null)
            .Include(t => t.SubTasks)
            .Include(t => t.Attachments)
            .ToListAsync();

        return View(tasks);
    }

    // GET: /Tasks/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;
        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId)
            .Include(t => t.Attachments)
            .ToListAsync();

        var totalTasks = tasks.Count;
        var completedTasks = tasks.Count(t => t.IsCompleted);
        var pendingTasks = totalTasks - completedTasks;
        
        var overdueTasks = tasks.Count(t => !t.IsCompleted && t.EndDate.HasValue && t.EndDate.Value.Date < today);

        double completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks * 100 : 0;

        var lowCount = tasks.Count(t => t.Priority == TaskPriority.Low);
        var mediumCount = tasks.Count(t => t.Priority == TaskPriority.Medium);
        var highCount = tasks.Count(t => t.Priority == TaskPriority.High);

        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var task in tasks)
        {
            foreach (var tag in task.HashtagList)
            {
                if (tagCounts.TryGetValue(tag, out int val))
                    tagCounts[tag] = val + 1;
                else
                    tagCounts[tag] = 1;
            }
        }
        var popularTags = tagCounts.OrderByDescending(kv => kv.Value).Take(6).ToList();

        var upcoming = tasks
            .Where(t => !t.IsCompleted && t.EndDate.HasValue && t.EndDate.Value.Date >= today)
            .OrderBy(t => t.EndDate)
            .Take(5)
            .ToList();

        var weekDayLabels = new List<string>();
        var tasksDueByDay = new List<int>();
        var tasksCreatedByDay = new List<int>();
        for (int i = 0; i < 7; i++)
        {
            var day = today.AddDays(i);
            weekDayLabels.Add(day.ToString("ddd"));
            tasksDueByDay.Add(tasks.Count(t =>
                !t.IsCompleted && t.EndDate.HasValue && t.EndDate.Value.Date == day));
            tasksCreatedByDay.Add(tasks.Count(t => t.CreatedAt.ToLocalTime().Date == day));
        }

        var monthStart = new DateTime(today.Year, today.Month, 1);
        var gridStart = monthStart.AddDays(-(int)monthStart.DayOfWeek);
        var calendarDays = new List<DashboardCalendarDay>();
        for (int i = 0; i < 42; i++)
        {
            var date = gridStart.AddDays(i);
            calendarDays.Add(new DashboardCalendarDay
            {
                Date = date,
                IsToday = date == today,
                IsCurrentMonth = date.Month == today.Month,
                TaskCount = tasks.Count(t =>
                    t.EndDate.HasValue && t.EndDate.Value.Date == date && !t.IsCompleted)
            });
        }

        var vm = new DashboardViewModel
        {
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            OverdueTasks = overdueTasks,
            CompletionRate = completionRate,
            LowPriorityCount = lowCount,
            MediumPriorityCount = mediumCount,
            HighPriorityCount = highCount,
            PopularTags = popularTags,
            UpcomingTasks = upcoming,
            WeekDayLabels = weekDayLabels,
            TasksDueByDay = tasksDueByDay,
            TasksCreatedByDay = tasksCreatedByDay,
            CalendarDays = calendarDays,
            CalendarMonthLabel = today.ToString("MMMM yyyy")
        };

        return View(vm);
    }

    private async Task<List<SelectListItem>> GetParentOptionsAsync(int? excludeId)
    {
        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.ParentTaskId == null && t.Id != excludeId)
            .OrderBy(t => t.Title)
            .ToListAsync();

        return tasks.Select(t => new SelectListItem(t.Title, t.Id.ToString())).ToList();
    }

    private async Task SaveAttachmentAsync(int taskId, IFormFile file)
    {
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsDir, storedName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        _db.Attachments.Add(new TaskAttachment
        {
            TaskItemId = taskId,
            FileName = file.FileName,
            FilePath = storedName,
            FileSizeBytes = file.Length
        });

        await _db.SaveChangesAsync();
    }
}
