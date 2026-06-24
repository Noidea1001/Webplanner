CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);

CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "DisplayName" TEXT NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);

CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Tasks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Tasks" PRIMARY KEY AUTOINCREMENT,
    "Title" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Priority" INTEGER NOT NULL,
    "EndDate" TEXT NULL,
    "Hashtags" TEXT NULL,
    "IsCompleted" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    "ParentTaskId" INTEGER NULL,
    CONSTRAINT "FK_Tasks_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tasks_Tasks_ParentTaskId" FOREIGN KEY ("ParentTaskId") REFERENCES "Tasks" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Attachments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Attachments" PRIMARY KEY AUTOINCREMENT,
    "TaskItemId" INTEGER NOT NULL,
    "FileName" TEXT NOT NULL,
    "FilePath" TEXT NOT NULL,
    "FileSizeBytes" INTEGER NOT NULL,
    "UploadedAt" TEXT NOT NULL,
    CONSTRAINT "FK_Attachments_Tasks_TaskItemId" FOREIGN KEY ("TaskItemId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Comments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Comments" PRIMARY KEY AUTOINCREMENT,
    "TaskItemId" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "AuthorUserId" TEXT NOT NULL,
    CONSTRAINT "FK_Comments_Tasks_TaskItemId" FOREIGN KEY ("TaskItemId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_Attachments_TaskItemId" ON "Attachments" ("TaskItemId");

CREATE INDEX "IX_Comments_TaskItemId" ON "Comments" ("TaskItemId");

CREATE INDEX "IX_Tasks_ParentTaskId" ON "Tasks" ("ParentTaskId");

CREATE INDEX "IX_Tasks_UserId" ON "Tasks" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624074849_InitialCreate', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624075334_SyncPendingChanges', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624092432_Migrationinit', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "DataProtectionKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY AUTOINCREMENT,
    "FriendlyName" TEXT NULL,
    "Xml" TEXT NULL
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624100531_AddDataProtectionKeys', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624100803_AddDataProtectionKeys1', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624101001_AddDataProtectionKeyss', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624101935_FixIsCompletedColumn', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624123000_FixPostgresIdentityColumns', '10.0.9');

COMMIT;

BEGIN TRANSACTION;
CREATE TABLE "ef_temp_Tasks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Tasks" PRIMARY KEY AUTOINCREMENT,
    "CreatedAt" timestamp without time zone NOT NULL,
    "Description" TEXT NULL,
    "EndDate" timestamp without time zone NULL,
    "Hashtags" TEXT NULL,
    "IsCompleted" INTEGER NOT NULL,
    "ParentTaskId" INTEGER NULL,
    "Priority" INTEGER NOT NULL,
    "Title" TEXT NOT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_Tasks_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tasks_Tasks_ParentTaskId" FOREIGN KEY ("ParentTaskId") REFERENCES "Tasks" ("Id") ON DELETE RESTRICT
);

INSERT INTO "ef_temp_Tasks" ("Id", "CreatedAt", "Description", "EndDate", "Hashtags", "IsCompleted", "ParentTaskId", "Priority", "Title", "UserId")
SELECT "Id", "CreatedAt", "Description", "EndDate", "Hashtags", "IsCompleted", "ParentTaskId", "Priority", "Title", "UserId"
FROM "Tasks";

CREATE TABLE "ef_temp_DataProtectionKeys" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_DataProtectionKeys" PRIMARY KEY AUTOINCREMENT,
    "FriendlyName" TEXT NULL,
    "Xml" TEXT NULL
);

INSERT INTO "ef_temp_DataProtectionKeys" ("Id", "FriendlyName", "Xml")
SELECT "Id", "FriendlyName", "Xml"
FROM "DataProtectionKeys";

CREATE TABLE "ef_temp_Comments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Comments" PRIMARY KEY AUTOINCREMENT,
    "AuthorUserId" TEXT NOT NULL,
    "CreatedAt" timestamp without time zone NOT NULL,
    "TaskItemId" INTEGER NOT NULL,
    "Text" TEXT NOT NULL,
    CONSTRAINT "FK_Comments_Tasks_TaskItemId" FOREIGN KEY ("TaskItemId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Comments" ("Id", "AuthorUserId", "CreatedAt", "TaskItemId", "Text")
SELECT "Id", "AuthorUserId", "CreatedAt", "TaskItemId", "Text"
FROM "Comments";

CREATE TABLE "ef_temp_Attachments" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Attachments" PRIMARY KEY AUTOINCREMENT,
    "FileName" TEXT NOT NULL,
    "FilePath" TEXT NOT NULL,
    "FileSizeBytes" INTEGER NOT NULL,
    "TaskItemId" INTEGER NOT NULL,
    "UploadedAt" timestamp without time zone NOT NULL,
    CONSTRAINT "FK_Attachments_Tasks_TaskItemId" FOREIGN KEY ("TaskItemId") REFERENCES "Tasks" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_Attachments" ("Id", "FileName", "FilePath", "FileSizeBytes", "TaskItemId", "UploadedAt")
SELECT "Id", "FileName", "FilePath", "FileSizeBytes", "TaskItemId", "UploadedAt"
FROM "Attachments";

CREATE TABLE "ef_temp_AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value")
SELECT "UserId", "LoginProvider", "Name", "Value"
FROM "AspNetUserTokens";

CREATE TABLE "ef_temp_AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "AccessFailedCount" INTEGER NOT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "DisplayName" TEXT NULL,
    "Email" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "PasswordHash" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "SecurityStamp" TEXT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "UserName" TEXT NULL
);

INSERT INTO "ef_temp_AspNetUsers" ("Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName")
SELECT "Id", "AccessFailedCount", "ConcurrencyStamp", "DisplayName", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName"
FROM "AspNetUsers";

CREATE TABLE "ef_temp_AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserRoles" ("UserId", "RoleId")
SELECT "UserId", "RoleId"
FROM "AspNetUserRoles";

CREATE TABLE "ef_temp_AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId")
SELECT "LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId"
FROM "AspNetUserLogins";

CREATE TABLE "ef_temp_AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetUserClaims" ("Id", "ClaimType", "ClaimValue", "UserId")
SELECT "Id", "ClaimType", "ClaimValue", "UserId"
FROM "AspNetUserClaims";

CREATE TABLE "ef_temp_AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "ConcurrencyStamp" TEXT NULL,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL
);

INSERT INTO "ef_temp_AspNetRoles" ("Id", "ConcurrencyStamp", "Name", "NormalizedName")
SELECT "Id", "ConcurrencyStamp", "Name", "NormalizedName"
FROM "AspNetRoles";

CREATE TABLE "ef_temp_AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);

INSERT INTO "ef_temp_AspNetRoleClaims" ("Id", "ClaimType", "ClaimValue", "RoleId")
SELECT "Id", "ClaimType", "ClaimValue", "RoleId"
FROM "AspNetRoleClaims";

COMMIT;

PRAGMA foreign_keys = 0;

BEGIN TRANSACTION;
DROP TABLE "Tasks";

ALTER TABLE "ef_temp_Tasks" RENAME TO "Tasks";

DROP TABLE "DataProtectionKeys";

ALTER TABLE "ef_temp_DataProtectionKeys" RENAME TO "DataProtectionKeys";

DROP TABLE "Comments";

ALTER TABLE "ef_temp_Comments" RENAME TO "Comments";

DROP TABLE "Attachments";

ALTER TABLE "ef_temp_Attachments" RENAME TO "Attachments";

DROP TABLE "AspNetUserTokens";

ALTER TABLE "ef_temp_AspNetUserTokens" RENAME TO "AspNetUserTokens";

DROP TABLE "AspNetUsers";

ALTER TABLE "ef_temp_AspNetUsers" RENAME TO "AspNetUsers";

DROP TABLE "AspNetUserRoles";

ALTER TABLE "ef_temp_AspNetUserRoles" RENAME TO "AspNetUserRoles";

DROP TABLE "AspNetUserLogins";

ALTER TABLE "ef_temp_AspNetUserLogins" RENAME TO "AspNetUserLogins";

DROP TABLE "AspNetUserClaims";

ALTER TABLE "ef_temp_AspNetUserClaims" RENAME TO "AspNetUserClaims";

DROP TABLE "AspNetRoles";

ALTER TABLE "ef_temp_AspNetRoles" RENAME TO "AspNetRoles";

DROP TABLE "AspNetRoleClaims";

ALTER TABLE "ef_temp_AspNetRoleClaims" RENAME TO "AspNetRoleClaims";

COMMIT;

PRAGMA foreign_keys = 1;

BEGIN TRANSACTION;
CREATE INDEX "IX_Tasks_ParentTaskId" ON "Tasks" ("ParentTaskId");

CREATE INDEX "IX_Tasks_UserId" ON "Tasks" ("UserId");

CREATE INDEX "IX_Comments_TaskItemId" ON "Comments" ("TaskItemId");

CREATE INDEX "IX_Attachments_TaskItemId" ON "Attachments" ("TaskItemId");

CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");

CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");

CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");

CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");

CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");

COMMIT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260624123946_FixDatabaseIdentityAndDateTime', '10.0.9');

