# GardenGroup Incident Management System

## 1. Project Description

GardenGroup is an Incident Management System developed for the NoSQL course. Employees report incidents (tickets) to the Service Desk. Service Desk employees manage tickets, follow up on their status, manage employee records and monitor the workload on a dashboard.

The application uses **MongoDB** as its database.

## 2. Features

### Ticket management
- Create, read, update and delete tickets.
- Ticket details page with the reporter and assignee (MongoDB `$lookup` on the `employees` collection).
- Filter tickets by status.
- Find tickets assigned to a specific employee.
- Sort tickets by creation date (newest or oldest first).

### Employee management
- Create, read and update employee records.
- Deactivate employees (soft delete: the record is kept, `isActive` is set to `false`).
- Duplicate protection: `employeeId` and `email` are unique.

### Dashboard
- Ticket status percentages, calculated with a MongoDB aggregation pipeline.
- Ticket count per assigned employee (aggregation with `$group` and `$lookup`).

### Planned
- Login and role-based access.

## 3. Technologies Used

| Area | Technology |
|---|---|
| Database | MongoDB (MongoDB.Driver for .NET) |
| Language | C# |
| Framework | ASP.NET Core MVC (.NET 10) |
| Version control | Git and GitHub |

## 4. Getting Started

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

## 5. Project Documentation

The `docs/` directory contains the project documentation:
- Database design and development decisions.
- CRUD implementation and query explanations.
- Index choices linked to queries, with `explain()` evidence.
- Alternative solutions and trade-offs.
- Distribution of work among team members.
