# PLM-EHC deployment kit

Everything needed to rebuild the PLM web app (ASP.NET MVC 5, .NET Framework 4.6.1,
EF6, SQL Server) on a fresh Windows server from source and a database backup.

```
Deployment/
  Build/Build-Package.ps1                 build machine: NuGet restore, Release publish, checks, zip
  Database/01-Restore-Database.sql        SQL: restore any .bak, files moved to this server's folders
  Database/02-Create-AppLogin.sql         SQL: app login/user, re-links orphaned users, least-privilege rights
  Database/03-Review-Environment-Settings.sql  SQL: lists AppSettings rows that still point at the old servers
  Database/Upgrade-Database.ps1           one command: backup, then 10 + 20 + 25, then both checks
  Database/10-Upgrade-Schema.sql          SQL: adds missing tables/columns (generated, additive only)
  Database/20-Upgrade-MasterData.sql      SQL: adds missing master data (generated, insert-only)
  Database/25-EHC-Gap-MasterData.sql      SQL: EHC keys the code needs that no repo script created
  Database/30-Verify-MasterData.sql       SQL: lists keys the code looks up that the DB lacks
  Database/90-Verify-Schema.sql           SQL: compares the DB with the tables/columns this code expects
  Database/tools/                         generators for 10, 20, 30 and 90
  IIS/Install-Site.ps1                    web server: IIS features, app pool, site, bindings, permissions
  IIS/Set-WebConfig.ps1                   web server: connection strings + production hardening in Web.config
```

## What you need

| Where | Requirement |
|---|---|
| Build machine | Windows, Visual Studio 2019/2022 or Build Tools with **ASP.NET and web development build tools**, .NET 4.6.1 targeting pack (or 4.8 dev pack), git |
| Web server | Windows Server 2016+ with IIS, .NET Framework 4.8 (4.6.1 minimum), PowerShell 5.1, run as Administrator |
| SQL Server | Same or **newer** major version than the server the .bak came from (a backup cannot be restored to an older version). `sqlcmd` on the machine you run the scripts from |
| Databases | Main PLM DB (`eServicesDbContext`) and the email/SMS queue DB `Cesar` (`CesarDbContext`) |

## Order of work

### 1. Build the package (build machine)

```powershell
git checkout RealEstate-Development      # or the branch you are releasing
.\Deployment\Build\Build-Package.ps1
```

Produces `Deployment\out\PLM-EHC_<branch>_<commit>_<time>.zip` plus a copy of these
scripts in `Deployment\out\Deployment`. The build fails if a view or PDF template is
missing from the output, if `wkhtmltopdf.exe` is missing, or if debug compilation is on.
Generated documents (signed leases, debit orders, uploads) are stripped: they contain
personal data and belong to the live server, not to a release.

### 2. Restore the databases (SQL Server)

Copy the .bak somewhere the **SQL Server service account** can read (local disk of the
SQL box is simplest). Then, from a prompt with `sqlcmd`:

```bat
cd Deployment\Database

sqlcmd -S SQLHOST\INSTANCE -E -b -i 01-Restore-Database.sql ^
  -v DbName="PropertyLeaseManagementRealEstate" BakPath="D:\Backups\PLM.bak" DataDir="DEFAULT" LogDir="DEFAULT" Replace="0"

sqlcmd -S SQLHOST\INSTANCE -E -b -i 01-Restore-Database.sql ^
  -v DbName="Cesar" BakPath="D:\Backups\Cesar.bak" DataDir="DEFAULT" LogDir="DEFAULT" Replace="0"
```

`Replace="1"` overwrites an existing DB of the same name (disconnects users first).
The script sets the compatibility level to the new server's, hands ownership to `sa`,
and lists users orphaned by the move.

If there is no Cesar backup, create it empty from source control instead:
`DatabaseScripts\Cesar_IntialCreationScript_v1.0.20190605a.sql` then
`DatabaseScripts\Cesar_IntialDataScript_v1.0.20190605a.sql`.

### 3. App login (SQL Server)

Run once per database. SQL login (what the app has used so far, e.g. `siyakhokha`):

```bat
sqlcmd -S SQLHOST\INSTANCE -E -b -i 02-Create-AppLogin.sql -v DbName="PropertyLeaseManagementRealEstate" LoginName="siyakhokha" LoginPassword="<password>"
sqlcmd -S SQLHOST\INSTANCE -E -b -i 02-Create-AppLogin.sql -v DbName="Cesar" LoginName="siyakhokha" LoginPassword="<password>"
```

Windows auth instead: `LoginPassword="WINDOWS"` with `LoginName="DOMAIN\svc_plm"` (or
`DOMAIN\WEBSERVER$` when IIS runs as ApplicationPoolIdentity on another machine).
The password must not contain a single quote. Rights granted: read, write, EXECUTE. Not db_owner.

### 4. Check the schema matches the code

```bat
sqlcmd -S SQLHOST\INSTANCE -E -W -i 90-Verify-Schema.sql -v DbName="PropertyLeaseManagementRealEstate"
```

- **MigrationState MATCH, no missing tables/columns**: done, go to step 5.
- **Missing tables/columns**: the backup is older than the code. See
  [Bringing an existing database up to date](#bringing-an-existing-database-up-to-date).

### 5. Point environment settings at the new servers

```bat
sqlcmd -S SQLHOST\INSTANCE -E -W -i 03-Review-Environment-Settings.sql -v DbName="PropertyLeaseManagementRealEstate"
```

Update any URL/path/IP rows that still name the old environment (report server,
upload folders, bill folders, API endpoints). Passwords are masked in the output.

### 6. Install the site (web server, elevated PowerShell)

```powershell
cd Deployment\IIS
.\Install-Site.ps1 -PackageZip D:\Drop\PLM-EHC_....zip                   # site "PLM" on port 80, C:\inetpub\PLM
.\Set-WebConfig.ps1 -SitePath C:\inetpub\PLM -SqlServer 'SQLHOST\INSTANCE' `
    -MainDatabase PropertyLeaseManagementRealEstate -CesarDatabase Cesar `
    -SqlCredential (Get-Credential siyakhokha)                             # or -IntegratedSecurity
```

Useful options: `-HostName plm.example.gov.za`, `-HttpsCertThumbprint <thumbprint>` (+ `-Https`
on Set-WebConfig), `-AppPoolCredential (Get-Credential DOMAIN\svc_plm)`, `-EncryptConnectionStrings`.

`Set-WebConfig.ps1` also removes the disabled dev connection strings (they carry
plaintext passwords), turns off debug compilation, sets `customErrors` to RemoteOnly and
locks `/elmah` to the *Super Administrators* role. It finishes with a live connection test.

### 7. Smoke test

1. Browse to the site, log in as an administrator.
2. Open an existing lease application and generate a PDF (exercises Rotativa/wkhtmltopdf and PDFTemplates).
3. Upload a document on a service request (exercises folder write permissions).
4. `/elmah` should demand login; after the tests it should show no new errors.

### Redeploying a new build later

Build a new zip and run `Install-Site.ps1` again. It backs the current site up to
`C:\inetpub\PLM_backup_<time>`, keeps the live `Web.config` and every upload/generated
document folder, and replaces the code. Roll back by copying the backup folder back.

## Bringing an existing database up to date

For a database that already exists (a dev copy, a restored backup), one command does it:

```powershell
cd Deployment\Database
.\Upgrade-Database.ps1 -Server localhost -Database PropertyLeaseManagementRealEstateFinal
# SQL login instead of Windows auth:  -SqlCredential (Get-Credential sa)
```

It takes a COPY_ONLY backup first, then runs, stopping at the first error:

| Step | Script | What it does |
|---|---|---|
| 1 | `10-Upgrade-Schema.sql` | Creates missing tables (keys, indexes, FKs) and adds missing columns. Additive only. Generated from the EF model plus the C# models for tables/columns that were added by hand SQL (RE_*, AuditTrail_*, LeaseTerminations refund fields, ...). |
| 2 | `20-Upgrade-MasterData.sql` | Inserts missing statuses, status types, action types, responsibility types, tracker messages, email texts, document types and checklists, roles, tenant-training slides and exam questions, complaint / payment transgression / service request lookups, and missing AppSettings keys. Insert-only, matched on natural key, FKs resolved by key. |
| 3 | `25-EHC-Gap-MasterData.sql` | Eight EHC keys the code uses that no seed script in the repo ever created (hand-written, each traced to its code path). |
| 4 | `90-Verify-Schema.sql` | Must list no missing tables or columns. |
| 5 | `30-Verify-MasterData.sql` | Lists every key the code looks up that the DB still lacks. Keys starting `re_` are Real Estate (DPRE); a few belong to modules EHC never opens. |

Every script is idempotent: a second run adds nothing. Logs go to `Deployment\Database\logs\<time>\`.
Existing rows are never changed, so names or AppSettings values someone edited on dev stay as they are.
New AppSettings rows carry dev values; review them with `03-Review-Environment-Settings.sql`.

How `20-Upgrade-MasterData.sql` was built: the repo's ~25 seed scripts (statuses, UC021-UC030,
complaints, training, maintenance, ...) were run in order on a scratch SQL Server 2019 database,
with their bugs fixed on the way (hard-coded ids, wrong column names, `USE` of other databases,
a transaction that swallowed its own error). `tools/generate_masterdata.py` then exported the
combined result as one clean script. Re-generate it the same way when new seed scripts are added.

## Notes

- `__MigrationHistory` and the real schema were allowed to drift during the Real Estate
  work (see `AI Handover/05_DATABASE_UPDATES_LOG.md`). Trust `90-Verify-Schema.sql` over the
  migration table.
- The app does not create or migrate the database on start-up (`SetInitializer(null)`).
- `BillingDbContext`, `ValueAssistDbContext`, `IntegrationEntities`, `eBilling6Months` and
  `CesarBulkDbContext` have no connection string in Web.config today; the PLM screens
  do not use them.
- After changing models, run `python3 Deployment/Database/tools/generate_schema_check.py`
  to refresh `90-Verify-Schema.sql`.
