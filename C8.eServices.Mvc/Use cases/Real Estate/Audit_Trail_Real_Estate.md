# PLM System - Audit Trail & Data Modifications Guide (Real Estate & EHC)

This document explains the architecture, security isolation, database schema, and integration hooks of the PLM Audit Trail system. 

---

## 1. System Architecture Overview

The system uses a **hybrid audit model** consisting of two main layers:
1. **Global Action Auditing (MVC Action Filter & Helper Hooks)**: Captures user sessions, page navigations, administrative commands, logins, logouts, role updates, and password resets.
2. **Granular Data Modification Auditing (DbContext Interceptor)**: Automatically records before/after state modifications on every database entity implementing the `IAuditable` interface.

```
       [ HTTP Request / Form Submit ]
                     │
         ┌───────────┴───────────┐
         ▼                       ▼
┌──────────────────┐   ┌──────────────────┐
│ MVC Action Filter│   │ Controllers /    │
│  (Page views &   │   │ Account / Roles  │
│   Form Submits)  │   │  (Manual Hooks)  │
└────────┬─────────┘   └─────────┬────────┘
         │                       │
         └───────────┬───────────┘
                     ▼
             [ AuditTrailHelper ]
                     │
                     ▼
         [ Write via SaveChangesWithoutAudit() ]
                     │
 ┌───────────────────┼───────────────────┐
 ▼                   ▼                   ▼
[UserLogins]    [Activities]       [PasswordResets]
                                   [RoleModifications]
```

---

## 2. Security & Department-Level Isolation

To satisfy data privacy requirements between departments (e.g., Real Estate vs. Ekurhuleni Housing Company - EHC), the system enforces strict department-level isolation:

- **EHC Staff** (assigned to EHC DepartmentId = 4) cannot view Real Estate records.
- **Real Estate Staff** (assigned to Real Estate DepartmentId = 3) cannot view EHC records.
- **Super Administrators / Back Office Admins** have system-wide access and bypass the filter.

### How it is enforced:
1. **Write-Time**: When `AuditTrailHelper` logs an activity, it resolves the actor's `DepartmentId` from `SystemUsers` and persists it directly in the record.
2. **Query-Time**: In [AuditTrailController.cs](file:///C:/REPO/PLM V1/PLM-EHC/C8.eServices.Mvc/Controllers/AuditTrailController.cs), the `GetDepartmentFilter()` helper is checked:
   - If it returns a department ID, all SQL queries are filtered to only return records belonging to that `DepartmentId`.
   - If it returns `null` (Admins), the query shows all records.
   - If the operation fails or is unauthorized, it returns `-1` (forces empty set to prevent leakage).

---

## 3. Database Schema

Four custom tables track high-level events, while the main `Audits` table tracks column-level modifications.

### Table 1: `AuditTrail_UserLogins`
Logs authentication events.
```sql
CREATE TABLE [dbo].[AuditTrail_UserLogins] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SystemUserId] INT NOT NULL,
    [DepartmentId] INT NULL,
    [EventDateTime] DATETIME NOT NULL,
    [EventType] VARCHAR(50) NOT NULL, -- 'Login', 'Logout'
    [IsSuccessful] BIT NOT NULL,
    [FailureReason] NVARCHAR(250) NULL,
    [IPAddress] VARCHAR(45) NULL,
    FOREIGN KEY ([SystemUserId]) REFERENCES [SystemUsers]([Id])
);
```

### Table 2: `AuditTrail_Activities`
Logs page hits, form submits, and general transactions.
```sql
CREATE TABLE [dbo].[AuditTrail_Activities] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [SystemUserId] INT NULL,
    [DepartmentId] INT NULL,
    [ActivityDateTime] DATETIME NOT NULL,
    [Controller] VARCHAR(100) NOT NULL,
    [Action] VARCHAR(100) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [ActivityType] VARCHAR(50) NOT NULL, -- 'PageView', 'FormSubmit', 'Export'
    [IsAdminAction] BIT NOT NULL DEFAULT 0,
    [IPAddress] VARCHAR(45) NULL,
    FOREIGN KEY ([SystemUserId]) REFERENCES [SystemUsers]([Id])
);
```

### Table 3: `AuditTrail_PasswordResets`
Logs password modifications.
```sql
CREATE TABLE [dbo].[AuditTrail_PasswordResets] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TargetSystemUserId] INT NOT NULL,
    [TargetDepartmentId] INT NULL,
    [ResetBySystemUserId] INT NOT NULL,
    [ResetDateTime] DATETIME NOT NULL,
    [ResetType] VARCHAR(50) NOT NULL, -- 'AdminReset', 'SelfReset'
    [IPAddress] VARCHAR(45) NULL,
    FOREIGN KEY ([TargetSystemUserId]) REFERENCES [SystemUsers]([Id]),
    FOREIGN KEY ([ResetBySystemUserId]) REFERENCES [SystemUsers]([Id])
);
```

### Table 4: `AuditTrail_RoleModifications`
Logs access level updates.
```sql
CREATE TABLE [dbo].[AuditTrail_RoleModifications] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [TargetSystemUserId] INT NOT NULL,
    [TargetDepartmentId] INT NULL,
    [ModifiedBySystemUserId] INT NOT NULL,
    [ModificationDateTime] DATETIME NOT NULL,
    [ModificationType] VARCHAR(50) NOT NULL, -- 'RoleChanged'
    [PreviousRoleName] NVARCHAR(MAX) NULL,
    [NewRoleName] NVARCHAR(MAX) NULL,
    [IPAddress] VARCHAR(45) NULL,
    FOREIGN KEY ([TargetSystemUserId]) REFERENCES [SystemUsers]([Id]),
    FOREIGN KEY ([ModifiedBySystemUserId]) REFERENCES [SystemUsers]([Id])
);
```

### Table 5: `Audits` (Field-Level Audit)
Pre-existing table that stores property-level modifications for entities implementing `IAuditable` when `SaveChanges()` is called.
```sql
CREATE TABLE [dbo].[Audits] (
    [AuditId] INT IDENTITY(1,1) PRIMARY KEY,
    [Action] VARCHAR(50) NOT NULL, -- 'Added', 'Modified', 'Deleted'
    [PrimaryKey] INT NOT NULL, -- Record Primary Key ID
    [TableName] VARCHAR(100) NOT NULL, -- Database table name
    [ColumnName] VARCHAR(100) NOT NULL, -- Modified property name
    [OriginalValue] NVARCHAR(MAX) NULL,
    [CurrentValue] NVARCHAR(MAX) NULL,
    [AuditBySystemUserId] INT NULL,
    [AuditDateTime] DATETIME NOT NULL,
    [IPAddress] VARCHAR(45) NULL
);
```

---

## 6. Key Integration Points

### Global MVC Filter Registration
Registered in `Global.asax.cs`:
```csharp
GlobalFilters.Filters.Add(new AuditTrailActionFilter());
```
This catches all standard page hits (`PageView`) and posts (`FormSubmit`) without modifications to individual controllers.

### Preventing Recursive Audit Loops
Because audit records are also database entries, saving them must not trigger another audit entry. Inside `eServicesDbContext.cs`, we bypass entity change-tracking for audits by invoking:
```csharp
public int SaveChangesWithoutAudit()
{
    return base.SaveChanges();
}
```
All code in `AuditTrailHelper.cs` saves logs exclusively through this method.

---

## 7. How to Change or Add Audit Hooks

### A. To Log a Page View or Custom User Transaction Manually
Invoke the static `LogActivity` helper:
```csharp
AuditTrailHelper.LogActivity(
    systemUserId: currentUserId,
    controller: "RealEstate",
    action: "OnboardProperty",
    description: "Successfully onboarded new property: " + propertyName,
    activityType: "FormSubmit",
    isAdminAction: false
);
```

### B. To Audit a Password Reset
Under the password reset post action:
```csharp
AuditTrailHelper.LogPasswordReset(
    targetUserId: targetUser.Id,
    resetByUserId: adminUser.Id,
    resetType: "AdminReset"
);
```

### C. To Audit Role or Access Modification
Under the user role assignment controller:
```csharp
AuditTrailHelper.LogRoleModification(
    targetUserId: targetUser.Id,
    modifiedByUserId: currentAdmin.Id,
    modificationType: "RoleChanged",
    previousRoles: "Clerks",
    newRoles: "Property Lease Management Senior Clerk"
);
```
