using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Data;
using WebPlanner.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
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


// AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

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

// ==================== DATABASE MIGRATION + SEEDING (FINAL STRONG FIX) ====================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();

    try
    {
        if (!app.Environment.IsDevelopment())
        {
            Console.WriteLine("🛠️ Running STRONG schema reset and type conversion for Neon...");

            // ១. បង្ខំឱ្យ EF Core បង្កើត Schema មូលដ្ឋានជាមុនសិន ប្រសិនបើវាជា DB ទទេ
            await db.Database.EnsureCreatedAsync();

            // ២. រត់កូដ SQL បំប្លែងប្រភេទទិន្នន័យពី SQLite ទៅជា PostgreSQL ពិតប្រាកដ
            await db.Database.ExecuteSqlRawAsync(@"
                DO $$
                BEGIN
                    -- កែសម្រួលតារាង DataProtectionKeys ឡើងវិញឱ្យត្រឹមត្រូវ
                    DROP TABLE IF EXISTS ""DataProtectionKeys"";
                    CREATE TABLE ""DataProtectionKeys"" (
                        ""Id"" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                        ""FriendlyName"" text NULL,
                        ""Xml"" text NULL
                    );

                    -- ដំណោះស្រាយដាច់ស្រឡះសម្រាប់បញ្ហាការចុះឈ្មោះ (EmailConfirmed Integer -> Boolean)
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'AspNetUsers') THEN
                        ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""EmailConfirmed"" TYPE boolean USING (""EmailConfirmed""::text::boolean);
                        ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""PhoneNumberConfirmed"" TYPE boolean USING (""PhoneNumberConfirmed""::text::boolean);
                        ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""TwoFactorEnabled"" TYPE boolean USING (""TwoFactorEnabled""::text::boolean);
                        ALTER TABLE ""AspNetUsers"" ALTER COLUMN ""LockoutEnabled"" TYPE boolean USING (""LockoutEnabled""::text::boolean);
                    END IF;

                    -- ដំណោះស្រាយដាច់ស្រឡះសម្រាប់បញ្ហា Calendar (DateTime Fix)
                    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'Tasks') THEN
                        -- ប្តូរទៅជាប្រភេទ timestamp ធម្មតា ដើម្បីកុំឱ្យទាស់គ្នាជាមួយ SQLite ពេលរត់ local
                        ALTER TABLE ""Tasks"" ALTER COLUMN ""IsCompleted"" TYPE boolean USING (""IsCompleted""::text::boolean);
                        ALTER TABLE ""Tasks"" ALTER COLUMN ""EndDate"" TYPE timestamp without time zone USING (""EndDate""::timestamp without time zone);
                        ALTER TABLE ""Tasks"" ALTER COLUMN ""CreatedAt"" TYPE timestamp without time zone USING (""CreatedAt""::timestamp without time zone);
                        
                        -- បង្ខំឱ្យ Id ក្លាយជា Identity សម្រាប់កូដអូតូ
                        IF NOT EXISTS (
                            SELECT 1 FROM information_schema.columns 
                            WHERE table_name = 'Tasks' AND column_name = 'Id' AND is_identity = 'YES'
                        ) THEN
                            ALTER TABLE ""Tasks"" ALTER COLUMN ""Id"" ADD GENERATED BY DEFAULT AS IDENTITY;
                        END IF;
                    END IF;

                END $$;
            ");

            Console.WriteLine("✅ All Postgres schema conversions applied successfully on Neon!");
        }
        else
        {
            // បើនៅលើម៉ាស៊ីនខ្លួនឯង (Development) ឱ្យវារត់ Migration ធម្មតា
            await db.Database.MigrateAsync();
            Console.WriteLine("✅ Local migrations completed successfully");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Migration error: {ex.Message}");
    }

    // Seed Admin Role
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
            Console.WriteLine("✅ Admin role seeded");
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