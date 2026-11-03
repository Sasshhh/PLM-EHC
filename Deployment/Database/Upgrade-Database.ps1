<#
.SYNOPSIS
  Brings an existing PLM database up to what the code on this branch needs:
  backup -> schema -> master data -> EHC gap rows -> both checks.

.DESCRIPTION
  Runs, in order, stopping at the first error:
    1. BACKUP DATABASE ... WITH COPY_ONLY (skip with -SkipBackup)
    2. 10-Upgrade-Schema.sql        missing tables and columns (additive only)
    3. 20-Upgrade-MasterData.sql    statuses, action types, tracker messages, document types,
                                    checklists, email texts, roles, training slides/questions,
                                    complaint / transgression / service request lookups (insert-only)
    4. 25-EHC-Gap-MasterData.sql    the few EHC keys no repo script ever created
    5. 90-Verify-Schema.sql         must report no missing tables/columns
    6. 30-Verify-MasterData.sql     lists any key the code needs that is still missing
  Every step is idempotent, so re-running is safe. Output goes to .\logs\<timestamp>\.

.EXAMPLE
  .\Upgrade-Database.ps1                                     # localhost, Windows auth, PropertyLeaseManagementRealEstateFinal
  .\Upgrade-Database.ps1 -Server '.\SQL2019' -Database MyCopy
  .\Upgrade-Database.ps1 -Server 10.2.2.17\submodules -SqlCredential (Get-Credential sa)
#>
[CmdletBinding()]
param(
    [string]$Server = 'localhost',
    [string]$Database = 'PropertyLeaseManagementRealEstateFinal',
    [pscredential]$SqlCredential,
    [string]$BackupFolder,
    [switch]$SkipBackup
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw 'sqlcmd not found. Install "Microsoft Command Line Utilities for SQL Server" or run from a machine with SSMS.'
}
$here  = $PSScriptRoot
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
$logs  = Join-Path $here "logs\$stamp"
New-Item -ItemType Directory -Force -Path $logs | Out-Null

$auth = @(if ($SqlCredential) { '-U', $SqlCredential.UserName, '-P', $SqlCredential.GetNetworkCredential().Password } else { '-E' })

function Invoke-Sql([string]$name, [string[]]$extra) {
    $log = Join-Path $logs "$name.log"
    Write-Host ("==> {0}" -f $name) -ForegroundColor Cyan
    & sqlcmd -S $Server @auth -b -I -W @extra -o $log
    $code = $LASTEXITCODE
    Get-Content $log | Where-Object { $_ -match 'added|Created table|Added column|Msg |ERROR|finished|applied|complete' } | Select-Object -Last 15 | ForEach-Object { Write-Host "    $_" }
    if ($code -ne 0) { throw "$name failed (exit $code). See $log" }
}

# 1. Backup
if (-not $SkipBackup) {
    $q = "SET NOCOUNT ON; SELECT CONVERT(nvarchar(4000), SERVERPROPERTY('InstanceDefaultBackupPath'))"
    if (-not $BackupFolder) { $BackupFolder = (& sqlcmd -S $Server @auth -h-1 -W -Q $q | Select-Object -First 1).Trim() }
    $bak = Join-Path $BackupFolder "$Database`_before_upgrade_$stamp.bak"
    Invoke-Sql '00-Backup' @('-Q', "BACKUP DATABASE [$Database] TO DISK = N'$bak' WITH COPY_ONLY, INIT, STATS = 25")
    Write-Host "    Backup: $bak  (on the SQL Server machine)"
}

# 2-4. Upgrade
foreach ($f in '10-Upgrade-Schema', '20-Upgrade-MasterData', '25-EHC-Gap-MasterData') {
    Invoke-Sql $f @('-i', (Join-Path $here "$f.sql"), '-v', "DbName=$Database")
}

# 5-6. Checks
Invoke-Sql '90-Verify-Schema' @('-i', (Join-Path $here '90-Verify-Schema.sql'), '-v', "DbName=$Database")
Invoke-Sql '30-Verify-MasterData' @('-i', (Join-Path $here '30-Verify-MasterData.sql'), '-v', "DbName=$Database")

Write-Host "`nSchema check  : $(Join-Path $logs '90-Verify-Schema.log')"
Write-Host "Master data   : $(Join-Path $logs '30-Verify-MasterData.log')"
Write-Host 'In the master-data check, keys starting re_ belong to the Real Estate (DPRE) module, not EHC.'
Write-Host 'Done.' -ForegroundColor Green
