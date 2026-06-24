using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Models;

namespace WebPlanner.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<TaskComment> Comments => Set<TaskComment>();
    public DbSet<TaskAttachment> Attachments => Set<TaskAttachment>();

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);

    // === FIX FOR DATAPROTECTIONKEYS ===
    builder.Entity<DataProtectionKey>(entity =>
    {
        entity.HasKey(k => k.Id);
        entity.Property(k => k.Id).UseIdentityByDefaultColumn();
    });

    // Your existing configuration
    builder.Entity<TaskItem>(entity =>
    {
        // កែប្រែត្រង់នេះ៖ បន្ថែម ValueGeneratedOnAdd() ដើម្បីប្រាប់ EF ឱ្យច្បាស់ថាកុំផ្ញើ Id ទៅ Database
        entity.Property(t => t.Id)
              .UseIdentityByDefaultColumn()
              .ValueGeneratedOnAdd(); 

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

    // === ដំណោះស្រាយដាច់ស្រឡះសម្រាប់បញ្ហា CALENDAR (DATETIME TIMEZONE) ===
    // កូដនេះនឹងបង្ខំឱ្យរាល់ជួរឈរប្រភេទ DateTime ទាំងអស់នៅក្នុង Database ប្តូរទៅជាប្រភេទ Timestamp ធម្មតា (ទោះជា UTC ឬមិន UTC ក៏មិនគាំងដែរ)
    foreach (var entityType in builder.Model.GetEntityTypes())
    {
        var properties = entityType.GetProperties()
            .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?));
        
        foreach (var property in properties)
        {
            property.SetColumnType("timestamp without time zone");
        }
    }
}
}
