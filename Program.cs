using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

var builder = WebApplication.CreateBuilder(args);

// ==================== RENDER / PRODUCTION FIXES ====================
builder.WebHost.UseUrls("http://0.0.0.0:8080");

if (string.IsNullOrEmpty(builder.Environment.EnvironmentName) || 
    builder.Environment.EnvironmentName == "Production")
{
    builder.Environment.EnvironmentName = "Production";
}

Console.WriteLine($"🚀 Running in: {builder.Environment.EnvironmentName} mode");

// ==================== DATABASE ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString ?? "Data Source=webplanner.db");
    }
    else
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Missing PostgreSQL connection string on Render!");

        options.UseNpgsql(connectionString);
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});

// ==================== MVC SERVICES ====================
var mvcBuilder = builder.Services.AddControllersWithViews();

// Only enable runtime compilation in Development (prevents inotify error)
if (builder.Environment.IsDevelopment())
{
    mvcBuilder.AddRazorRuntimeCompilation();
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==================== DB MIGRATION & SEED ====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (app.Environment.IsDevelopment())
    {
        db.Database.EnsureCreated();
    }
    else
    {
        db.Database.Migrate();
    }

    // Seed Admin Role
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new IdentityRole("Admin"));
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