<#
.SYNOPSIS
  Installs or upgrades the PLM site in IIS from a package zip.

.DESCRIPTION
  Run as Administrator on the Windows Server web host. Safe to re-run: on an
  upgrade it keeps the live Web.config and every user-generated file.

    1. Installs the IIS role + ASP.NET 4.x features (Windows Server).
    2. Checks .NET Framework 4.6.1 or later is present.
    3. Unpacks the zip into <SitePath>\..\<SiteName>_releases\<timestamp>, then
       mirrors it onto SitePath. On upgrade the previous SitePath is copied to
       <SitePath>_backup_<timestamp> first (rollback = copy it back).
       Preserved on upgrade: Web.config (unless -UsePackageWebConfig), App_Data, PDFFiles, Templates (generated
       docs), Uploads, UploadedFiles, Section49PDFs, Logs, JSON, FileUpload,
       Content\Lease_Agreements.
    4. Creates/updates the app pool (.NET CLR v4.0, Integrated, 64-bit,
       AlwaysRunning, no idle timeout, daily recycle at 02:00).
    5. Creates/updates the site and its HTTP (and optional HTTPS) binding.
    6. Grants the app pool identity Read on the site and Modify on the folders
       the app writes to.

  Then run Set-WebConfig.ps1 for connection strings.

.EXAMPLE
  .\Install-Site.ps1 -PackageZip D:\Drop\PLM-EHC_RealEstate-Development_6dfb07d_20261102-1000.zip

.EXAMPLE
  # Own host name on 443 with an existing certificate, running as a domain account
  .\Install-Site.ps1 -PackageZip D:\Drop\PLM.zip -HostName plm.example.gov.za -HttpsCertThumbprint 'AB12...' `
      -AppPoolCredential (Get-Credential 'DOMAIN\svc_plm')
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$PackageZip,
    [string]$SiteName = 'PLM',
    [string]$AppPoolName = 'PLM',
    [string]$SitePath = 'C:\inetpub\PLM',
    [int]$HttpPort = 80,
    [string]$HostName = '',
    [string]$HttpsCertThumbprint = '',
    [int]$HttpsPort = 443,
    [pscredential]$AppPoolCredential,
    [switch]$SkipFeatureInstall,
    [switch]$UsePackageWebConfig
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Step($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }

$id = [Security.Principal.WindowsIdentity]::GetCurrent()
if (-not ([Security.Principal.WindowsPrincipal]$id).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw 'Run this script from an elevated (Administrator) PowerShell.'
}
$PackageZip = (Resolve-Path $PackageZip).Path
$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'

# --- 1. IIS features ---------------------------------------------------------
if (-not $SkipFeatureInstall) {
    Step 'Installing IIS and ASP.NET 4.x features'
    $features = 'Web-Server', 'Web-WebServer', 'Web-Common-Http', 'Web-Default-Doc', 'Web-Static-Content',
                'Web-Http-Errors', 'Web-Http-Redirect', 'Web-Health', 'Web-Http-Logging', 'Web-Request-Monitor',
                'Web-Performance', 'Web-Stat-Compression', 'Web-Dyn-Compression', 'Web-Security', 'Web-Filtering',
                'Web-Windows-Auth', 'Web-App-Dev', 'Web-Net-Ext45', 'Web-Asp-Net45', 'Web-ISAPI-Ext', 'Web-ISAPI-Filter',
                'Web-Mgmt-Tools', 'Web-Mgmt-Console', 'NET-Framework-45-ASPNET'
    if (Get-Command Install-WindowsFeature -ErrorAction SilentlyContinue) {
        $r = Install-WindowsFeature -Name $features
        if ($r.RestartNeeded -eq 'Yes') { Write-Warning 'A restart is needed to finish installing IIS features. Restart, then re-run with -SkipFeatureInstall.' }
    } else {
        Write-Warning 'Install-WindowsFeature not available (not Windows Server). Enable IIS + ASP.NET 4.x in "Windows Features" manually, then re-run with -SkipFeatureInstall.'
    }
}

# --- 2. .NET Framework ----------------------------------------------------------
Step 'Checking .NET Framework'
$release = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full' -ErrorAction SilentlyContinue).Release
if (-not $release -or $release -lt 394254) { throw ".NET Framework 4.6.1 or later is required (found release key '$release'). Install .NET Framework 4.8." }
Write-Host ".NET Framework release key $release OK"

Import-Module WebAdministration

# --- 3. Files ------------------------------------------------------------------
Step "Deploying files to $SitePath"
$releases = Join-Path (Split-Path $SitePath) ("{0}_releases" -f (Split-Path $SitePath -Leaf))
$staging = Join-Path $releases $stamp
New-Item -ItemType Directory -Force -Path $staging | Out-Null
Expand-Archive -Path $PackageZip -DestinationPath $staging -Force
if (-not (Test-Path (Join-Path $staging 'bin\C8.eServices.Mvc.dll'))) { throw "Package does not look like a PLM build (bin\C8.eServices.Mvc.dll missing)." }

$preserve = 'App_Data', 'PDFFiles', 'Templates', 'Uploads', 'UploadedFiles', 'Section49PDFs', 'Logs', 'JSON', 'FileUpload', 'Content\Lease_Agreements'
$upgrade = Test-Path (Join-Path $SitePath 'Web.config')
if ($upgrade) {
    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        if ((Get-WebAppPoolState $AppPoolName).Value -ne 'Stopped') { Stop-WebAppPool $AppPoolName; Start-Sleep 3 }
    }
    $backup = "{0}_backup_{1}" -f $SitePath, $stamp
    Write-Host "Upgrade: backing up current site to $backup"
    & robocopy $SitePath $backup /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Backup copy failed (robocopy $LASTEXITCODE)" }
    if ($UsePackageWebConfig) {
        Write-Warning 'Using the package Web.config: run Set-WebConfig.ps1 next, the site has no DB settings until you do.'
    } else {
        # Keep the live Web.config (it holds the server's connection strings).
        Copy-Item (Join-Path $SitePath 'Web.config') (Join-Path $staging 'Web.config') -Force
        Write-Host 'Kept the existing Web.config. If this release changed Web.config (new assembly bindingRedirects,'
        Write-Host 'new appSettings), re-run with -UsePackageWebConfig and then Set-WebConfig.ps1. Compare:'
        Write-Host "  $backup\Web.config  vs  Web.config inside $PackageZip"
    }
}
# Mirror code, but never touch preserved data folders.
$xd = $preserve | ForEach-Object { (Join-Path $SitePath $_); (Join-Path $staging $_) }
& robocopy $staging $SitePath /MIR /R:2 /W:2 /NFL /NDL /NJH /NP /XD @xd | Out-Null
if ($LASTEXITCODE -ge 8) { throw "File copy failed (robocopy $LASTEXITCODE)" }
# First install: seed the preserved folders from the package (templates etc.).
foreach ($d in $preserve) {
    $src = Join-Path $staging $d; $dst = Join-Path $SitePath $d
    if (-not (Test-Path $dst)) { New-Item -ItemType Directory -Force -Path $dst | Out-Null }
    if (Test-Path $src) { & robocopy $src $dst /E /XC /XN /XO /R:1 /W:1 /NFL /NDL /NJH /NJS /NP | Out-Null }   # add new files only
}
foreach ($d in 'Uploads\ServiceRequests\Documents', 'Uploads\PaymentTransgressions\Letters', 'Uploads\PaymentTransgressions\Documents',
               'UploadedFiles\ComplaintInvestigation', 'UploadedFiles\ComplaintEvidence', 'Section49PDFs\ST', 'Section49PDFs\FT') {
    New-Item -ItemType Directory -Force -Path (Join-Path $SitePath $d) | Out-Null
}

# --- 4. App pool ---------------------------------------------------------------
Step "Configuring app pool '$AppPoolName'"
if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) { New-WebAppPool -Name $AppPoolName | Out-Null }
$pool = "IIS:\AppPools\$AppPoolName"
Set-ItemProperty $pool managedRuntimeVersion 'v4.0'
Set-ItemProperty $pool managedPipelineMode 'Integrated'
Set-ItemProperty $pool enable32BitAppOnWin64 $false
Set-ItemProperty $pool startMode 'AlwaysRunning'
Set-ItemProperty $pool processModel.idleTimeout ([TimeSpan]::Zero)
Set-ItemProperty $pool processModel.loadUserProfile $true
Set-ItemProperty $pool recycling.periodicRestart.time ([TimeSpan]::Zero)
Clear-ItemProperty $pool -Name recycling.periodicRestart.schedule
New-ItemProperty $pool -Name recycling.periodicRestart.schedule -Value @{ value = '02:00:00' } | Out-Null
if ($AppPoolCredential) {
    Set-ItemProperty $pool processModel -Value @{ userName = $AppPoolCredential.UserName; password = $AppPoolCredential.GetNetworkCredential().Password; identityType = 3 }
    $identity = $AppPoolCredential.UserName
} else {
    Set-ItemProperty $pool processModel.identityType 4   # ApplicationPoolIdentity
    $identity = "IIS AppPool\$AppPoolName"
}
Write-Host "App pool identity: $identity"

# --- 5. Site + bindings ----------------------------------------------------------
Step "Configuring site '$SiteName'"
if (-not (Get-Website -Name $SiteName)) {
    $conflict = Get-WebBinding -Port $HttpPort -HostHeader $HostName -ErrorAction SilentlyContinue
    if ($conflict) { Write-Warning "Port $HttpPort / host '$HostName' is already bound by another site; it will be created stopped. Change -HttpPort or -HostName, or stop the other site." }
    New-Website -Name $SiteName -PhysicalPath $SitePath -ApplicationPool $AppPoolName -Port $HttpPort -HostHeader $HostName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$SiteName" physicalPath $SitePath
    Set-ItemProperty "IIS:\Sites\$SiteName" applicationPool $AppPoolName
}
Set-ItemProperty "IIS:\Sites\$SiteName" applicationDefaults.preloadEnabled $true
if ($HttpsCertThumbprint) {
    if (-not (Get-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader $HostName)) {
        New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader $HostName -SslFlags ([int][bool]$HostName)
    }
    $cert = Get-Item "Cert:\LocalMachine\My\$HttpsCertThumbprint"
    (Get-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -HostHeader $HostName).AddSslCertificate($cert.Thumbprint, 'My')
    Write-Host "HTTPS bound on $HttpsPort with $($cert.Subject)"
}

# --- 6. Permissions -----------------------------------------------------------------
Step "Granting file permissions to $identity"
& icacls $SitePath /grant "${identity}:(OI)(CI)RX" /T /C /Q | Out-Null
foreach ($d in $preserve) {
    & icacls (Join-Path $SitePath $d) /grant "${identity}:(OI)(CI)M" /T /C /Q | Out-Null
}
# wkhtmltopdf runs as a child process of the app pool.
& icacls (Join-Path $SitePath 'Rotativa') /grant "${identity}:(OI)(CI)RX" /T /C /Q | Out-Null

# --- Start ---------------------------------------------------------------------
Step 'Starting'
Start-WebAppPool $AppPoolName -ErrorAction SilentlyContinue
Start-Website $SiteName -ErrorAction SilentlyContinue
Get-Website -Name $SiteName | Format-Table Name, State, PhysicalPath, @{n = 'Bindings'; e = { ($_.bindings.Collection | ForEach-Object { $_.bindingInformation }) -join ', ' } } -AutoSize
Write-Host "Done. Next: .\Set-WebConfig.ps1 -SitePath $SitePath ... (first install, or when the DB moves)." -ForegroundColor Green
Write-Host 'Uploads/generated documents are NOT in the package: copy them from the old server into the preserved folders if needed.'
