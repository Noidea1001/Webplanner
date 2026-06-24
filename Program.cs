using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

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
    }
    else
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Missing PostgreSQL connection string!");

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
builder.Services.AddControllersWithViews();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==================== DB SETUP ====================
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