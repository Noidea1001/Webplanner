# Web Planner

ASP.NET Core 8 MVC app + REST API for managing tasks, sub-tasks, priorities, due dates,
hashtags, comments and file attachments. Bootstrap 5 UI throughout. SQLite database.
Simple email/password login via ASP.NET Core Identity.

## Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run it

```bash
cd WebPlanner
dotnet restore
dotnet run
```

Then open the URL shown in the console (usually `https://localhost:5001`).
The SQLite database (`webplanner.db`) and schema are created automatically on first run
(no `dotnet ef` migrations needed).

Register an account at `/Account/Register`, then start adding tasks.

## What's included

- **MVC pages** (Bootstrap 5, `Views/Tasks/*`): list, create, edit, delete, details
  (with subtasks/comments/attachments), search, and a calendar view filterable by
  Day / Week / Month / Year.
- **REST API** (`/api/tasks/*`, same cookie auth as the website):
  - `GET    /api/tasks`                 – list your tasks
  - `GET    /api/tasks/{id}`            – get one task
  - `POST   /api/tasks`                 – create
  - `PUT    /api/tasks/{id}`            – update
  - `DELETE /api/tasks/{id}`            – delete
  - `GET    /api/tasks/search?q=&priority=&hashtag=`
  - `GET    /api/tasks/calendar?range=day|week|month|year&date=2026-06-22`
  - `POST   /api/tasks/{id}/comments`
  - `POST   /api/tasks/{id}/attachments` (multipart/form-data, field `file`)
  - `DELETE /api/tasks/attachments/{attachmentId}`

  Log in via the website first (it sets an auth cookie) and the same browser session
  can call the API endpoints directly, e.g. `fetch('/api/tasks')`.

## Project structure

```
Controllers/        MVC controllers (Tasks, Account, Home)
Controllers/Api/     REST API controller (TasksApiController)
Models/              EF Core entities (TaskItem, TaskComment, TaskAttachment, ApplicationUser)
Models/ViewModels/   Form/view models for MVC pages
Models/Dtos/          DTOs for the JSON API
Data/                EF Core DbContext
Views/               Razor views (Bootstrap 5 via CDN)
wwwroot/              Static files, uploaded attachments stored under wwwroot/uploads
```

## Notes / next steps
- Uses `EnsureCreated()` instead of EF migrations for simplicity. To switch to migrations:
  `dotnet ef migrations add Initial` then `dotnet ef database update`.
- Password rules are relaxed (min 6 chars) for easy local testing — tighten in
  `Program.cs` (`AddIdentity` options) for production use.
- File uploads are stored on disk under `wwwroot/uploads` with a GUID-prefixed filename.
