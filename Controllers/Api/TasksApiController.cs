using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;
using WebPlanner.Models.Dtos;

namespace WebPlanner.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public TasksApiController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _db = db;
        _userManager = userManager;
        _env = env;
    }

    private string CurrentUserId => _userManager.GetUserId(User)!;

    // GET /api/tasks?topLevelOnly=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll([FromQuery] bool topLevelOnly = false)
    {
        var query = _db.Tasks.Where(t => t.UserId == CurrentUserId);
        if (topLevelOnly) query = query.Where(t => t.ParentTaskId == null);

        var tasks = await query
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .OrderBy(t => t.EndDate)
            .ToListAsync();

        return Ok(tasks.Select(TaskDto.FromEntity));
    }

    // GET /api/tasks/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> GetById(int id)
    {
        var task = await _db.Tasks
            .Include(t => t.SubTasks)
            .Include(t => t.Comments)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);

        if (task == null) return NotFound();
        return Ok(TaskDto.FromEntity(task));
    }

    // GET /api/tasks/search?q=report&priority=High&hashtag=work
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] TaskPriority? priority,
        [FromQuery] string? hashtag)
    {
        var query = _db.Tasks.Where(t => t.UserId == CurrentUserId);

        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(t => t.Title.Contains(q) || (t.Description != null && t.Description.Contains(q)));

        if (priority.HasValue)
            query = query.Where(t => t.Priority == priority.Value);

        if (!string.IsNullOrWhiteSpace(hashtag))
        {
            var tag = hashtag.TrimStart('#');
            query = query.Where(t => t.Hashtags != null && t.Hashtags.Contains(tag));
        }

        var results = await query.Include(t => t.Attachments).ToListAsync();
        return Ok(results.Select(TaskDto.FromEntity));
    }

    // GET /api/tasks/calendar?range=week&date=2026-06-22
    [HttpGet("calendar")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> Calendar([FromQuery] string range = "week", [FromQuery] DateTime? date = null)
    {
        var anchor = (date ?? DateTime.Today).Date;
        DateTime start, end;

        switch (range.ToLowerInvariant())
        {
            case "day":
                start = anchor; end = anchor.AddDays(1).AddTicks(-1);
                break;
            case "month":
                start = new DateTime(anchor.Year, anchor.Month, 1);
                end = start.AddMonths(1).AddTicks(-1);
                break;
            case "year":
                start = new DateTime(anchor.Year, 1, 1);
                end = start.AddYears(1).AddTicks(-1);
                break;
            default: // week
                int diff = (7 + (anchor.DayOfWeek - DayOfWeek.Monday)) % 7;
                start = anchor.AddDays(-diff);
                end = start.AddDays(7).AddTicks(-1);
                break;
        }

        var tasks = await _db.Tasks
            .Where(t => t.UserId == CurrentUserId && t.EndDate != null && t.EndDate >= start && t.EndDate <= end)
            .ToListAsync();

        return Ok(tasks.Select(TaskDto.FromEntity));
    }

    // POST /api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] TaskCreateUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        // A parent task, if specified, must belong to the current user
        if (dto.ParentTaskId.HasValue)
        {
            var parentExists = await _db.Tasks.AnyAsync(t => t.Id == dto.ParentTaskId && t.UserId == CurrentUserId);
            if (!parentExists) return BadRequest("ParentTaskId does not refer to an existing task you own.");
        }

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            EndDate = dto.EndDate,
            Hashtags = dto.Hashtags,
            IsCompleted = dto.IsCompleted,
            ParentTaskId = dto.ParentTaskId,
            UserId = CurrentUserId
        };

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = task.Id }, TaskDto.FromEntity(task));
    }

    // PUT /api/tasks/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TaskCreateUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        if (dto.ParentTaskId == id) return BadRequest("A task cannot be its own parent.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Priority = dto.Priority;
        task.EndDate = dto.EndDate;
        task.Hashtags = dto.Hashtags;
        task.IsCompleted = dto.IsCompleted;
        task.ParentTaskId = dto.ParentTaskId;

        await _db.SaveChangesAsync();
        return Ok(TaskDto.FromEntity(task));
    }

    // DELETE /api/tasks/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _db.Tasks.Include(t => t.SubTasks)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        foreach (var sub in task.SubTasks) sub.ParentTaskId = null;

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/tasks/5/comments
    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] CommentCreateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        var comment = new TaskComment
        {
            TaskItemId = id,
            Text = dto.Text,
            AuthorUserId = CurrentUserId
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return Ok(CommentDto.FromEntity(comment));
    }

    // POST /api/tasks/5/attachments  (multipart/form-data, field name "file")
    [HttpPost("{id:int}/attachments")]
    public async Task<ActionResult<AttachmentDto>> UploadAttachment(int id, IFormFile file)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();
        if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var fullPath = Path.Combine(uploadsDir, storedName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
            await file.CopyToAsync(stream);

        var attachment = new TaskAttachment
        {
            TaskItemId = id,
            FileName = file.FileName,
            FilePath = storedName,
            FileSizeBytes = file.Length
        };
        _db.Attachments.Add(attachment);
        await _db.SaveChangesAsync();

        return Ok(AttachmentDto.FromEntity(attachment));
    }

    // DELETE /api/tasks/attachments/7
    [HttpDelete("attachments/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        var attachment = await _db.Attachments
            .Include(a => a.TaskItem)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskItem!.UserId == CurrentUserId);
        if (attachment == null) return NotFound();

        var fullPath = Path.Combine(_env.WebRootPath, "uploads", attachment.FilePath);
        if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

        _db.Attachments.Remove(attachment);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/tasks/{id}/move
    [HttpPost("{id:int}/move")]
    public async Task<IActionResult> MoveTask(int id, [FromBody] TaskMoveDto dto)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.UserId == CurrentUserId);
        if (task == null) return NotFound();

        task.Priority = dto.Priority;
        task.IsCompleted = dto.IsCompleted;
        await _db.SaveChangesAsync();

        return Ok(TaskDto.FromEntity(task));
    }
}
