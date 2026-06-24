using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore; 
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

// ... rest of your code

var builder = WebApplication.CreateBuilder(args);

// ==================== RENDER FIXES ====================
builder.WebHost.UseUrls("http://0.0.0.0:8080");

Console.WriteLine($"🚀 Running in: {builder.Environment.EnvironmentName} mode");

// ==================== DATABASE ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString ?? "Data Source=webplanner.db");
        Console.WriteLine("✅ Using SQLite (Development)");
    }
    else
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("❌ ERROR: No PostgreSQL connection string found!");
            throw new InvalidOperationException("Missing PostgreSQL connection string on Render!");
        }
        options.UseNpgsql(connectionString);
        Console.WriteLine("✅ Using PostgreSQL (Production)");
    }

    options.ConfigureWarnings(w => w.Ignore(
        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// ==================== IDENTITY ====================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName("WebPlanner");

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

// ==================== MVC ====================
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

var app = builder.Build();

// ==================== DB SETUP + ERROR HANDLING ====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (app.Environment.IsDevelopment())
        {
            // ណែនាំឱ្យប្រើ Migrate() ដូចគ្នាដើម្បីជៀសវាងបញ្ហា Migration ថ្ងៃក្រោយ
            db.Database.Migrate();
            Console.WriteLine("✅ SQLite database and migrations ready");
        }
        else
        {
            Console.WriteLine("🔄 Applying PostgreSQL migrations...");
            db.Database.Migrate();
            Console.WriteLine("✅ PostgreSQL migrations completed successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database Error: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        throw;
    }

   // Create Admin Role
    try
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        
        if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
        {
            roleManager.CreateAsync(new IdentityRole("Admin")).GetAwaiter().GetResult();
            Console.WriteLine("✅ Admin role created");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Role creation warning: {ex.Message}");
    }

}

Console.WriteLine("🚀 Application startup completed successfully");

// ==================== MIDDLEWARE ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.MapControllers();

app.Run();