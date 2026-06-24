using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Models;

namespace WebPlanner.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();
    public DbSet<TaskAttachment> Attachments => Set<TaskAttachment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TaskItem>(entity =>
        {
            entity.HasOne(t => t.ParentTask)
                  .WithMany(t => t.SubTasks)
                  .HasForeignKey(t => t.ParentTaskId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Comments)
                  .WithOne(c => c.TaskItem!)
                  .HasForeignKey(c => c.TaskItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Attachments)
                  .WithOne(a => a.TaskItem!)
                  .HasForeignKey(a => a.TaskItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
