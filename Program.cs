using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

var builder = WebApplication.CreateBuilder(args);

// === STRONG ENVIRONMENT FIX ===
string environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] 
                  ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? (builder.Environment.IsDevelopment() ? "Development" : "Production");

if (environment == "Development" || 
    Environment.CommandLine.Contains("Development"))
{
    builder.Environment.EnvironmentName = "Development";
}

var env = builder.Environment;

Console.WriteLine($"🚀 Running in: {env.EnvironmentName} mode");

// === Database Configuration ===
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (env.IsDevelopment())
    {
        options.UseSqlite(connectionString ?? "Data Source=webplanner.db");
        Console.WriteLine("✅ Using SQLite (Development)");
    }
    else
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("❌ Missing PostgreSQL connection string in Production!");
        }
        options.UseNpgsql(connectionString);
        Console.WriteLine("✅ Using PostgreSQL (Production)");
    }

    // === បិទការការពារ និងបង្ខំឱ្យរត់ចាក់ Table ===
    options.ConfigureWarnings(warnings => warnings.Ignore(
        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning
    ));
});

// === Identity ===
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options => {
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
});


// Services
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// === Database Setup ===
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (env.IsDevelopment())
    {
        db.Database.EnsureCreated();
        Console.WriteLine("📦 SQLite database ready");
    }
    else
    {
        // បញ្ជាឱ្យបង្កើត Tables លើ Neon Postgres ដោយស្វ័យប្រវត្តពេលឡើង Render
        db.Database.Migrate();
        Console.WriteLine("📦 PostgreSQL migrations applied");

        // លុប ឬបិទ DataSeeder ចោល ព្រោះនៅលើ Render គ្មាន File SQLite ឡើយ
        // DataSeeder.SeedFromSqliteToPostgres(scope.ServiceProvider);
    }
}

// Middleware
if (env.IsDevelopment())
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

// === កំណត់ Port សម្រាប់ Render ===
var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
app.Urls.Add($"http://*:{port}");

app.Run();
