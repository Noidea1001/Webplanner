using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

var builder = WebApplication.CreateBuilder(args);

// ==================== RENDER + PRODUCTION FIXES ====================
builder.WebHost.UseUrls("http://0.0.0.0:8080");

Console.WriteLine($"🚀 Environment: {builder.Environment.EnvironmentName}");

// ==================== DATABASE CONFIG ====================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? (builder.Environment.IsDevelopment() ? "Data Source=webplanner.db" : null);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
        Console.WriteLine("✅ Using SQLite for Development");
    }
    else
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("❌ PostgreSQL connection string is required in Production (Render)!");
        }
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(3); // Better resilience on Render
        });
        Console.WriteLine("✅ Using PostgreSQL for Production");
    }

    options.ConfigureWarnings(w => w.Ignore(
        Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// ==================== IDENTITY + DATA PROTECTION ====================
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
    .SetApplicationName("WebPlanner")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Important for HTTPS
});

// ==================== SERVICES ====================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // If using Identity UI

// Swagger (optional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==================== DATABASE MIGRATION + SEEDING ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        Console.WriteLine("🔄 Applying database migrations...");

        if (!app.Environment.IsDevelopment())
        {
            Console.WriteLine("🛠️  Running PostgreSQL compatibility fixes...");

            // Fix IsCompleted column type safely
            await db.Database.ExecuteSqlRawAsync(@"
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'Tasks' 
                          AND column_name = 'IsCompleted' 
                          AND data_type != 'boolean'
                    ) THEN
                        ALTER TABLE ""Tasks"" 
                        ALTER COLUMN ""IsCompleted"" TYPE boolean 
                        USING (CASE 
                            WHEN ""IsCompleted""::text IN ('1', 'true', 'yes') THEN true 
                            ELSE false 
                        END);
                        RAISE NOTICE 'Fixed IsCompleted column type';
                    END IF;
                END $$;
            ");

            Console.WriteLine("✅ PostgreSQL column type fix applied");
        }

        await db.Database.MigrateAsync();
        Console.WriteLine("✅ All migrations completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
        // Don't throw in production if it's a non-critical column fix
        if (!ex.Message.Contains("IsCompleted"))
            throw;
    }

    // Seed Admin Role
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            Console.WriteLine("✅ Admin role created");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Role seeding warning: {ex.Message}");
    }
}

// ==================== MIDDLEWARE PIPELINE ====================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
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

app.MapRazorPages(); // For Identity pages if used

Console.WriteLine("🚀 WebPlanner started successfully on Render!");
app.Run();