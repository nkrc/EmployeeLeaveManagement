# Employee Leave Management System

An ASP.NET Core MVC-based web application for managing employee leave requests. The system provides separate access for Admin and Employee users, allowing employees to apply for leave and administrators to review, approve, reject, and report leave requests.

## Features

### Admin Features

- Admin login
- Admin dashboard
- View total employees
- View total leave requests
- View pending, approved, and rejected request counts
- Add new employees
- Edit employee details
- Deactivate employees
- View all leave requests
- Filter leave requests by status
- Approve pending leave requests
- Reject pending leave requests
- Search leave requests by date range and status
- Export filtered reports to Excel
- Export filtered reports to PDF

### Employee Features

- Employee login
- Employee dashboard
- Apply for leave
- View own leave requests
- View leave request status
- Withdraw pending leave requests
- View pending, approved, rejected, and withdrawn requests

### Validations

- From Date cannot be later than To Date
- Same-day leave is supported
- Overlapping pending or approved leave requests are blocked
- Required field validation
- Email validation
- Duplicate employee email validation
- Only pending requests can be withdrawn
- Only pending requests can be approved or rejected

## Technology Stack

- **Backend:** ASP.NET Core MVC
- **Language:** C#
- **Framework:** .NET 9
- **Database:** Microsoft SQL Server
- **ORM:** Entity Framework Core
- **Frontend:** Razor Views, HTML, CSS, Bootstrap
- **Authentication:** Cookie Authentication
- **Excel Export:** ClosedXML
- **PDF Export:** QuestPDF

## Project Structure

```text
EmployeeLeaveManagement
│
├── Controllers
│   ├── AccountController.cs
│   ├── AdminController.cs
│   └── EmployeeController.cs
│
├── Data
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs
│
├── Models
│   ├── User.cs
│   └── LeaveRequest.cs
│
├── ViewModels
│   ├── LoginViewModel.cs
│   ├── EmployeeViewModel.cs
│   ├── LeaveRequestViewModel.cs
│   └── LeaveReportViewModel.cs
│
├── Views
│   ├── Account
│   │   ├── Login.cshtml
│   │   └── AccessDenied.cshtml
│   │
│   ├── Admin
│   │   ├── Dashboard.cshtml
│   │   ├── Employees.cshtml
│   │   ├── CreateEmployee.cshtml
│   │   ├── EditEmployee.cshtml
│   │   ├── LeaveRequests.cshtml
│   │   └── Reports.cshtml
│   │
│   ├── Employee
│   │   ├── Dashboard.cshtml
│   │   └── MyLeaves.cshtml
│   │
│   └── Shared
│       ├── _Layout.cshtml
│       └── _ValidationScriptsPartial.cshtml
│
├── wwwroot
│   ├── css
│   └── js
│
├── appsettings.json
├── Program.cs
└── EmployeeLeaveManagement.csproj
```

## Prerequisites

Install the following before running the project:

- Visual Studio 2022 or later
- .NET 9 SDK
- SQL Server Express or SQL Server Developer Edition
- SQL Server Management Studio
- Git, if cloning the project

## Database Configuration

Open:

```text
appsettings.json
```

Update the connection string according to your SQL Server configuration.

Example for SQL Server Express:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=EmployeeLeaveManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

Example for LocalDB:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=EmployeeLeaveManagementDb;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

## NuGet Packages

The project uses the following packages:

```powershell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.EntityFrameworkCore.Design
Install-Package ClosedXML
Install-Package QuestPDF
```

## Database Migration

Open the **Package Manager Console** in Visual Studio and run:

```powershell
Add-Migration InitialCreate
Update-Database
```

Alternatively, using the .NET CLI:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The database will be created automatically based on the Entity Framework Core models.

## Default Login Credentials

The application seeds default users when the database is empty.

### Admin

```text
Email: admin@example.com
Password: admin123
Role: Admin
```

### Employee

```text
Email: employee@example.com
Password: emp123
Role: Employee
```

> These default passwords are provided for demonstration purposes. In a production application, passwords should be securely hashed.

## Running the Application

1. Clone or open the project in Visual Studio.
2. Configure the SQL Server connection string in `appsettings.json`.
3. Restore NuGet packages.
4. Run the database migration.
5. Build the project.
6. Run the application using Visual Studio or the following command:

```bash
dotnet run
```

7. Open the URL shown in the terminal or Visual Studio.
8. Login using one of the default credentials.

## Application Workflow

### Employee Workflow

```text
Employee Login
      |
      v
Employee Dashboard
      |
      v
Apply Leave
      |
      v
Date and Overlap Validation
      |
      v
Leave Request Created with Pending Status
      |
      v
Admin Reviews Request
      |
      +-------------------+
      |                   |
      v                   v
Approved              Rejected
```

### Admin Workflow

```text
Admin Login
     |
     v
Admin Dashboard
     |
     +--------------------+
     |                    |
     v                    v
Manage Employees     Manage Leave Requests
     |                    |
     v                    v
Add/Edit/Deactivate  Approve/Reject
                          |
                          v
                     Generate Reports
                          |
                          +----------+
                          |          |
                          v          v
                        Excel       PDF
```

## Leave Overlap Validation

The system prevents an employee from applying for overlapping leave when an existing request is either:

- Pending
- Approved

For example:

| Existing Leave | New Leave | Result |
|---|---|---|
| 14 September – 14 September | 14 September – 14 September | Blocked |
| 14 September – 16 September | 15 September – 15 September | Blocked |
| 14 September – 16 September | 13 September – 15 September | Blocked |
| 14 September – 16 September | 16 September – 18 September | Blocked |
| 14 September – 16 September | 17 September – 18 September | Allowed |

When an overlapping request is submitted, the application displays an error toast notification.

Example:

```text
Leave request already exists for the selected date or date range.
```

## Toast Notifications

The application uses Bootstrap toast notifications for user feedback.

## Reporting

The Reports page allows the administrator to:

- Select From Date
- Select To Date
- Select Status
- Search filtered leave requests
- Export the filtered result to Excel
- Export the filtered result to PDF

The export files contain:

- Employee Name
- Employee Email
- From Date
- To Date
- Reason
- Applied At
- Status


```

