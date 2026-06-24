using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebPlanner.Migrations
{
    /// <inheritdoc />
    public partial class FixIsCompletedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // --- រក្សាទុកកូដបំប្លែងជួរឈរផ្សេងទៀត (UserId, Title, Priority...) ទុកដដែល ---
    migrationBuilder.AlterColumn<string>(name: "UserId", table: "Tasks", type: "text", nullable: true);
    migrationBuilder.AlterColumn<string>(name: "Title", table: "Tasks", type: "character varying(200)", nullable: false);
    // ... (កូដចាស់របស់អ្នក) ...

    // ==================== ដំណោះស្រាយសម្រាប់ ISCOMPLETED ====================
    
    // ពិនិត្យមើលថា តើបច្ចុប្បន្នកូដកំពុងរត់នៅលើ Neon (PostgreSQL) ឬ SQLite?
    if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
    {
        // សម្រាប់ Render + Neon: ប្រើប្រាស់បញ្ជា SQL ពិសេសដើម្បីបង្ខំឱ្យបំប្លែងប្រភេទលេខទៅជា Boolean
        migrationBuilder.Sql("ALTER TABLE \"Tasks\" ALTER COLUMN \"IsCompleted\" TYPE boolean USING (\"IsCompleted\"::integer::boolean);");
    }
    else
    {
        // សម្រាប់ Local + SQLite: ទុកឱ្យ EF Core ដំណើរការតាមធម្មតា (ព្រោះ SQLite គ្មានបញ្ហាជាមួយលេខ 0/1 ទេ)
        migrationBuilder.AlterColumn<bool>(
            name: "IsCompleted",
            table: "Tasks",
            type: "INTEGER", // SQLite ប្រើប្រាស់ INTEGER សម្រាប់តំណាងឱ្យ Boolean
            nullable: false,
            oldClrType: typeof(int));
    }
}

    }
}
