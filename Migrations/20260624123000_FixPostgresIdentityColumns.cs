using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebPlanner.Migrations;

public partial class FixPostgresIdentityColumns : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Tasks_Id_seq') THEN
                        CREATE SEQUENCE "Tasks_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "Tasks" ALTER COLUMN "Id" SET DEFAULT nextval('"Tasks_Id_seq"');
                ALTER SEQUENCE "Tasks_Id_seq" OWNED BY "Tasks"."Id";
                SELECT setval('"Tasks_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Tasks"), 0) + 1, false);
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Attachments_Id_seq') THEN
                        CREATE SEQUENCE "Attachments_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "Attachments" ALTER COLUMN "Id" SET DEFAULT nextval('"Attachments_Id_seq"');
                ALTER SEQUENCE "Attachments_Id_seq" OWNED BY "Attachments"."Id";
                SELECT setval('"Attachments_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Attachments"), 0) + 1, false);
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'Comments_Id_seq') THEN
                        CREATE SEQUENCE "Comments_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "Comments" ALTER COLUMN "Id" SET DEFAULT nextval('"Comments_Id_seq"');
                ALTER SEQUENCE "Comments_Id_seq" OWNED BY "Comments"."Id";
                SELECT setval('"Comments_Id_seq"', COALESCE((SELECT MAX("Id") FROM "Comments"), 0) + 1, false);
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'DataProtectionKeys_Id_seq') THEN
                        CREATE SEQUENCE "DataProtectionKeys_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "DataProtectionKeys" ALTER COLUMN "Id" SET DEFAULT nextval('"DataProtectionKeys_Id_seq"');
                ALTER SEQUENCE "DataProtectionKeys_Id_seq" OWNED BY "DataProtectionKeys"."Id";
                SELECT setval('"DataProtectionKeys_Id_seq"', COALESCE((SELECT MAX("Id") FROM "DataProtectionKeys"), 0) + 1, false);
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'AspNetRoleClaims_Id_seq') THEN
                        CREATE SEQUENCE "AspNetRoleClaims_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"AspNetRoleClaims_Id_seq"');
                ALTER SEQUENCE "AspNetRoleClaims_Id_seq" OWNED BY "AspNetRoleClaims"."Id";
                SELECT setval('"AspNetRoleClaims_Id_seq"', COALESCE((SELECT MAX("Id") FROM "AspNetRoleClaims"), 0) + 1, false);
            """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM pg_sequences WHERE schemaname = 'public' AND sequencename = 'AspNetUserClaims_Id_seq') THEN
                        CREATE SEQUENCE "AspNetUserClaims_Id_seq" START 1;
                    END IF;
                END $$;
                ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" SET DEFAULT nextval('"AspNetUserClaims_Id_seq"');
                ALTER SEQUENCE "AspNetUserClaims_Id_seq" OWNED BY "AspNetUserClaims"."Id";
                SELECT setval('"AspNetUserClaims_Id_seq"', COALESCE((SELECT MAX("Id") FROM "AspNetUserClaims"), 0) + 1, false);
            """);
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Tasks" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "Tasks_Id_seq" CASCADE;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Attachments" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "Attachments_Id_seq" CASCADE;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "Comments" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "Comments_Id_seq" CASCADE;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "DataProtectionKeys" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "DataProtectionKeys_Id_seq" CASCADE;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetRoleClaims" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "AspNetRoleClaims_Id_seq" CASCADE;
            """);

            migrationBuilder.Sql("""
                ALTER TABLE "AspNetUserClaims" ALTER COLUMN "Id" DROP DEFAULT;
                DROP SEQUENCE IF EXISTS "AspNetUserClaims_Id_seq" CASCADE;
            """);
        }
    }
}
