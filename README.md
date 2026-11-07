# GardenGroup Incident Management System

## 1. Project Description

GardenGroup is an Incident Management System developed for the NoSQL course (1918IN233A). Employees report incidents (tickets) to the Service Desk. Service Desk employees manage tickets, follow up on their status, manage employee records and monitor the workload on a dashboard.

The application uses **MongoDB** as its database and is built with **ASP.NET Core MVC** in C#.

## 2. Features

### Login and roles
- Log in with email and password, log out. Passwords are stored hashed with ASP.NET Core `PasswordHasher<Employee>`. A migration (`EmployeePasswordMigration`) converts old plain-text passwords at startup.
- Unknown users, inactive users and wrong passwords all get the same generic error message, so nobody can find out which email addresses exist.
- Roles: `employee` (regular) and `agent` / `manager` (Service Desk). The mapping lives in `Authorization/AppRoles.cs`; `UserClaimsExtensions` provides `User.IsServiceDesk()` and `User.GetEmployeeId()`.
- Forgot password (`ForgotPasswordService`): a reset link with a random 32-byte token that is valid for 30 minutes and works only once. The page gives the same answer whether or not the email address exists.

### Permissions
- `[Authorize]` on every controller, plus a Service Desk policy on the Service Desk actions (manage, edit, delete, transfer, escalate, close). Users without rights see an "Access denied" page.
- The user ID always comes from the login claims, never from a URL or form field. A regular employee's ticket list, search results and dashboard are filtered on their own ID.
- Opening someone else's ticket through the URL returns 403.
- The navigation menu depends on the role ("My tickets" vs. "Tickets"; "Employees" only for the Service Desk).
- Every POST has an anti-forgery token, the login page only accepts local return URLs, and POST actions use `[Bind]` with an explicit field list against over-posting.

### Ticket management
- Create, read, update and delete tickets. Delete is only done after a confirmation page.
- A regular employee creates tickets as themselves (the reporter is filled in automatically). The Service Desk can create a ticket on behalf of an employee.
- Validation with data annotations on the `Ticket` model. The deadline rules (required, not in the past) are checked server-side in the controller.
- Ticket details page with reporter and assignee (MongoDB `$lookup` on the `employees` collection). Missing data is shown as "Unassigned" or "Unknown employee".
- Invalid IDs return 400, missing tickets return 404.
- Assigning is only possible to an active Service Desk employee.
- **Transfer** a ticket to another employee (`TicketTransferService`). The service layer rejects unknown or inactive target employees.
- **Escalate** (Open or In progress → Escalated, sets `escalatedAt`) and **close** (any status except Closed) a ticket (`TicketEscalator`). The status update is compare-and-set, so two Service Desk employees acting at the same time cannot overwrite each other.
- One ticket list for both roles: a regular employee sees only their own tickets ("My tickets"), the Service Desk sees all tickets.
- Filter tickets by status; the Service Desk can also filter by assignee.
- Sort tickets by creation date (newest or oldest first).
- Sort tickets by priority, high to low or low to high (`TicketPrioritySorter`). The sorting runs in MongoDB and combines with all filters.
- Search tickets on words in the title and description, with AND and OR, newest first (`TicketSearcher`). Example: `printer AND offline OR wifi` is read as `(printer AND offline) OR (wifi)`. Search input is escaped (`Regex.Escape`) and limited to 100 characters and 20 words.
- Search and priority sorting are paginated (pages of 100, with a fixed sort order).

### Ticket statuses and priorities
- One definition of each: `Enums/Status.cs` (`Statuses`: Open, In_Progress, Escalated, Resolved, Closed) and `Enums/Priorities.cs` (`Priorities`: Low 0, Medium 1, High 2, Urgent 3). The model, repositories, dashboard pipelines and views all use these.
- Priority is stored as a number so that sorting in the database is correct (sorting on text would be alphabetical).
- Dashboard categories (`TicketCategory`) are derived from the status:

| Category | Rule |
|---|---|
| Open (not yet resolved) | Status Open, In_Progress or Escalated (also the default for anything unknown) |
| Resolved | Status Resolved, or `resolvedAt` is set (a ticket that was resolved and then closed stays resolved) |
| Closed without resolve | Status Closed and no `resolvedAt` |

A ticket with status Open always counts as Open, even if it still has an old `resolvedAt` (reopened ticket).

### Employee management
- The Service Desk can list, create, edit and deactivate employees.
- Deactivate instead of delete (soft delete): the record is kept and `isActive` is set to `false`. The employee can no longer log in or reset their password, no longer appears in the reporter / assignee / transfer lists, and the Service Desk can reactivate them. Their tickets stay in place and their name stays visible on ticket details and in the workload overview.
- Duplicate protection on three levels: controller (field messages), service, and unique indexes in MongoDB on `employeeId` and `email`.
- The create form suggests the highest employee number + 1.
- Password is required when creating and optional when editing (empty = unchanged). A new password also clears an open reset token.

### Dashboard
- Ticket status percentages (Open, Resolved, Closed without resolve), calculated with a MongoDB aggregation pipeline. Categories without tickets are shown as 0, and with zero tickets the page shows an empty-state message.
- Workload per assigned employee (aggregation with `$group` and `$lookup`). Tickets without an assignee are not counted.
- A regular employee sees a personal dashboard with only their own percentages. The Service Desk sees the dashboard for all tickets, including the workload. A regular employee never receives the workload data.

## 3. Individual Functionality (Deliverable 3)

Each individual feature is a separate class with an interface, registered in `Program.cs`.

| Team member | Feature | Main classes |
|---|---|---|
| Kyano | Forgot password | `ForgotPasswordService` / `IForgotPasswordService`, `AccountController.ForgotPassword` and `ResetPassword` |
| Carlo | Sort by priority | `TicketPrioritySorter` / `ITicketPrioritySorter`, `TicketSort`, `TicketSortOrder` |
| Emre | Escalate / close | `TicketEscalator` / `ITicketEscalator` |
| Mart | Search with AND / OR | `TicketSearcher` / `ITicketSearcher`, `TicketListQuery` |
| Frank | Transfer ticket | `TicketTransferService` / `ITicketTransferService`, `TicketController.Transfer` |

## 4. Architecture

The application uses layered MVC: **View ← Controller → Service → Repository → MongoDB**.

- **Controllers** handle HTTP requests, read the logged-in user and choose the view.
- **Services** contain the business rules (for example: a ticket can only be assigned to an active employee).
- **Repositories** contain all MongoDB queries.

Rules in services stay out of the controllers and can be tested without a web request.

## 5. Indexes

All indexes are created once at startup by `MongoIndexInitializer` (called from `Program.cs`). The old single-field indexes `status_1` and `assignedTo_1` are removed at startup because the compound indexes cover them.

| Index | Fields | Used for |
|---|---|---|
| `ix_status_createdAt` | status, createdAt | Filter by status, newest first |
| `ix_assignedTo_createdAt` | assignedTo, createdAt | Tickets of one employee, newest first |
| `createdAt_-1` | createdAt | Sort all tickets by date |
| `ix_reportedBy_status` | reportedBy, status | Dashboard: status percentages per reporter |
| `ix_reportedBy_createdAt` | reportedBy, createdAt | "My tickets" for a regular employee |
| `ix_priority_sort` | priority, createdAt, _id | Sort by priority (both directions) |
| `ix_reporter_priority_sort` | reportedBy, priority, createdAt, _id | Priority sort of own tickets |
| `ix_status_priority_sort` | status, priority, createdAt, _id | Priority sort within a status |
| `ux_ticketId` (unique) | ticketId | Unique ticket numbers, fast lookup of the highest number |
| `ux_employeeId` (unique) | employeeId | Duplicate protection |
| `ux_employee_email` (unique) | email | Duplicate protection |

Ticket numbers are created as highest `ticketId` + 1, with up to 3 retries on a duplicate-key error.

## 6. Technologies Used

| Area | Technology |
|---|---|
| Database | MongoDB (MongoDB.Driver for .NET) |
| Language | C# |
| Framework | ASP.NET Core MVC (.NET 10) |
| Front-end | Razor views, Bootstrap, `site.css` and `dashboard.css` |
| Configuration | .NET User Secrets |
| Version control | Git and GitHub |

## 7. Getting Started

### Prerequisites
- .NET 10 SDK
- Access to the MongoDB database (Atlas or local)

### Configure the connection string
The connection string is **not** stored in the repository. Each developer sets it locally with .NET User Secrets. Run this in the folder that contains `GardenGroup.csproj`:

```bash
dotnet user-secrets set "ConnectionStrings:MongoDb" "<your-mongodb-connection-string>"
```

Ask a teammate for the connection string. Never commit it to GitHub.

### Run the application
```bash
dotnet run
```

The app uses the database `helpdesk` with the collections `tickets` and `employees`. Indexes are created automatically when the application starts.

## 8. Known Limitations

- The forgot-password flow creates the reset link but does not send an email. The link is shown on the confirmation page, intended for a later mail integration. The token is stored as-is (storing a hash would be safer in case of a database leak).
- Ticket CRUD is synchronous, while employees, login, search, priority sorting and the dashboard are asynchronous. Making ticket CRUD async would require changing the repository, service and controller together.
- Search uses regex filters, which cannot use an index. This is acceptable for the current data volume; a MongoDB text index is the next step if the data grows.

## 9. Project Documentation

The `Docs/` directory contains the project documentation (Deliverable 2 and 3) covering:

- **Database design and technology choices:** MongoDB collections, document relationships, configuration, and the decision to store tickets and employees separately.
- **CRUD implementation:** Create, Read, Update, and Delete operations for tickets and employees, including the relevant repository and service methods, and the changes since Deliverable 2 (async employees, soft delete, compare-and-set status updates).
- **Architecture, authentication and roles:** Layered MVC, login, password hashing, roles and how permissions are enforced.
- **Queries and indexes:** Ticket filtering, sorting and searching, employee lookups, dashboard aggregations, and the indexes defined in `MongoIndexInitializer`.
- **Alternatives and trade-offs:** Database query strategies, compound indexes, ticket number generation, projections, aggregations, `$lookup`, deactivating vs. deleting employees, and the design choices of each team member.
- **Individual functionality and distribution of work:** Team members' responsibilities, features and their associated files and classes.
- **Testing and setup:** Test descriptions, MongoDB configuration, and instructions for running the application.
