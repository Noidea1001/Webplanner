using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPlanner.Models;

namespace WebPlanner.Data
{
    public static class DataSeeder
    {
        public static void SeedFromSqliteToPostgres(IServiceProvider serviceProvider)
        {
            // ១. កំណត់ទម្រង់ការភ្ជាប់ទៅកាន់ SQLite (ប្រភពទិន្នន័យចាស់)
            var sqliteOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite("Data Source=webplanner.db")
                .Options;

            // ២. បង្កើត Context
            using var postgresContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
            using var sqliteContext = new ApplicationDbContext(sqliteOptions);

            // ពិនិត្យមើលបើ PostgreSQL មានទិន្នន័យ Tasks រួចហើយ មិនបាច់បញ្ចូលទៀតទេ
            if (postgresContext.Tasks.Any())
            {
                Console.WriteLine("⚠️ PostgreSQL already has data. Skipping data migration.");
                return;
            }

            Console.WriteLine("⏳ Starting data migration from SQLite to PostgreSQL...");

            // បង្កើត Transaction ត្រឹមត្រូវ
            using var transaction = postgresContext.Database.BeginTransaction();

            try
            {
                // === ១. ផ្ទេរទិន្នន័យ Roles ===
                var sqliteRoles = sqliteContext.Roles.AsNoTracking().ToList();
                if (sqliteRoles.Any() && !postgresContext.Roles.Any())
                {
                    postgresContext.Roles.AddRange(sqliteRoles);
                    postgresContext.SaveChanges();
                    Console.WriteLine($"✅ Seeded {sqliteRoles.Count} Roles.");
                }

                // === ២. ផ្ទេរទិន្នន័យ Users ===
                var sqliteUsers = sqliteContext.Users.AsNoTracking().ToList();
                if (sqliteUsers.Any() && !postgresContext.Users.Any())
                {
                    postgresContext.Users.AddRange(sqliteUsers);
                    postgresContext.SaveChanges();
                    Console.WriteLine($"✅ Seeded {sqliteUsers.Count} Users.");
                }

                // === ៣. ផ្ទេរទិន្នន័យ UserRoles ===
                var sqliteUserRoles = sqliteContext.UserRoles.AsNoTracking().ToList();
                if (sqliteUserRoles.Any() && !postgresContext.UserRoles.Any())
                {
                    postgresContext.UserRoles.AddRange(sqliteUserRoles);
                    postgresContext.SaveChanges();
                    Console.WriteLine($"✅ Seeded {sqliteUserRoles.Count} User-Role relations.");
                }

                // === ៤. ផ្ទេរទិន្នន័យ Tasks ===
                var sqliteTasks = sqliteContext.Tasks.AsNoTracking().ToList();
                if (sqliteTasks.Any())
                {
                    postgresContext.Tasks.AddRange(sqliteTasks);
                    postgresContext.SaveChanges();
                    Console.WriteLine($"✅ Seeded {sqliteTasks.Count} Tasks.");
                }

                // បញ្ចប់យុទ្ធនាការដោយជោគជ័យ
                transaction.Commit();
                Console.WriteLine("🎉 All data migrated from SQLite to PostgreSQL successfully!");
            }
            catch (Exception ex)
            {
                // បើមានកំហុស វានឹងលុបចោលការធ្វើទាំងអស់ដើម្បីកុំឱ្យខូច Database
                transaction.Rollback();
                Console.WriteLine($"❌ Data migration failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"🔍 Inner Exception: {ex.InnerException.Message}");
                }
            }
            // === ៥. ផ្ទេរទិន្នន័យ Attachments ===
            var sqliteAttachments = sqliteContext.Attachments.AsNoTracking().ToList();
            if (sqliteAttachments.Any() && !postgresContext.Attachments.Any())
            {
                postgresContext.Attachments.AddRange(sqliteAttachments);
                postgresContext.SaveChanges();
                Console.WriteLine($"✅ Seeded {sqliteAttachments.Count} Attachments.");
            }

            // === ៦. ផ្ទេរទិន្នន័យ Comments ===
            var sqliteComments = sqliteContext.Comments.AsNoTracking().ToList();
            if (sqliteComments.Any() && !postgresContext.Comments.Any())
            {
                postgresContext.Comments.AddRange(sqliteComments);
                postgresContext.SaveChanges();
                Console.WriteLine($"✅ Seeded {sqliteComments.Count} Comments.");
            }

        }
    }
}
