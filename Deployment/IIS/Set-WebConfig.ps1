<#
.SYNOPSIS
  Writes the server-specific settings into the deployed Web.config.

.DESCRIPTION
  The Web.config in source control points at developers' machines and carries
  several disabled connection strings with plaintext passwords. This script, run
  on the web server after the files are in place:

    - backs up Web.config to Web.config.<timestamp>.bak
    - sets eServicesDbContext (main PLM DB) and CesarDbContext (email/SMS queue DB)
    - removes every other connection string (the disabled dev/test entries)
    - turns off debug compilation, sets customErrors to RemoteOnly
    - locks ELMAH (/elmah) to the "Super Administrators" role
    - with -Https, marks cookies Secure

  It never prints passwords. With -SqlCredential the password is stored in
  Web.config (as the app has always done); use -EncryptConnectionStrings to
  encrypt the section with DPAPI for this machine afterwards.

.EXAMPLE
  # SQL login (e.g. siyakhokha)
  .\Set-WebConfig.ps1 -SitePath C:\inetpub\PLM -SqlServer '10.2.2.17\submodules' `
      -MainDatabase PropertyLeaseManagementRealEstate -CesarDatabase Cesar -SqlCredential (Get-Credential siyakhokha)

.EXAMPLE
  # Windows auth as the app pool identity / service account
  .\Set-WebConfig.ps1 -SitePath C:\inetpub\PLM -SqlServer 'SQLHOST\INSTANCE' `
      -MainDatabase PropertyLeaseManagementRealEstate -CesarDatabase Cesar -IntegratedSecurity
#>
[CmdletBinding(DefaultParameterSetName = 'Sql')]
param(
    [Parameter(Mandatory)] [string]$SitePath,
    [Parameter(Mandatory)] [string]$SqlServer,
    [Parameter(Mandatory)] [string]$MainDatabase,
    [string]$CesarDatabase = 'Cesar',
    [Parameter(Mandatory, ParameterSetName = 'Sql')] [pscredential]$SqlCredential,
    [Parameter(Mandatory, ParameterSetName = 'Windows')] [switch]$IntegratedSecurity,
    [switch]$Https,
    [switch]$KeepOtherConnectionStrings,
    [switch]$EncryptConnectionStrings,
    [string]$AppPoolName = 'PLM'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$path = Join-Path $SitePath 'Web.config'
if (-not (Test-Path $path)) { throw "Web.config not found at $path" }
$backup = "$path.$(Get-Date -Format yyyyMMdd-HHmmss).bak"
Copy-Item $path $backup
Write-Host "Backed up to $backup"

$xml = New-Object xml
$xml.PreserveWhitespace = $true
$xml.Load($path)
$cfg = $xml.configuration

function Get-OrAdd([Xml.XmlNode]$parent, [string]$name) {
    $n = $parent.SelectSingleNode($name)
    if (-not $n) { $n = $parent.AppendChild($xml.CreateElement($name)) }
    $n
}

function New-ConnString([string]$db) {
    $b = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $b['Data Source'] = $SqlServer
    $b['Initial Catalog'] = $db
    $b['MultipleActiveResultSets'] = $true
    $b['Connect Timeout'] = 30
    if ($IntegratedSecurity) {
        $b['Integrated Security'] = $true
    } else {
        $b['User ID'] = $SqlCredential.UserName
        $b['Password'] = $SqlCredential.GetNetworkCredential().Password
        $b['Persist Security Info'] = $false
    }
    $b.ConnectionString
}

# --- connection strings -------------------------------------------------------
$cs = Get-OrAdd $cfg 'connectionStrings'
if ($cs.Attributes['configProtectionProvider']) { throw 'connectionStrings is encrypted. Decrypt first: aspnet_regiis -pdf connectionStrings <site path>' }
$wanted = [ordered]@{ eServicesDbContext = $MainDatabase; CesarDbContext = $CesarDatabase }
if (-not $KeepOtherConnectionStrings) {
    foreach ($n in @($cs.SelectNodes('add'))) {
        if (-not $wanted.Contains($n.GetAttribute('name'))) {
            Write-Host "  removing unused connection string '$($n.GetAttribute('name'))'"
            [void]$cs.RemoveChild($n)
        }
    }
}
foreach ($name in $wanted.Keys) {
    $n = $cs.SelectSingleNode("add[@name='$name']")
    if (-not $n) { $n = $cs.AppendChild($xml.CreateElement('add')); $n.SetAttribute('name', $name) }
    $n.SetAttribute('connectionString', (New-ConnString $wanted[$name]))
    $n.SetAttribute('providerName', 'System.Data.SqlClient')
    Write-Host "  $name -> $SqlServer / $($wanted[$name]) ($(if ($IntegratedSecurity) {'Windows auth'} else {'SQL login ' + $SqlCredential.UserName}))"
}

# --- system.web hardening -------------------------------------------------------
$sw = $cfg.SelectSingleNode('system.web')
$comp = Get-OrAdd $sw 'compilation'
if ($comp.HasAttribute('debug')) { $comp.RemoveAttribute('debug') }
$ce = Get-OrAdd $sw 'customErrors'
$ce.SetAttribute('mode', 'RemoteOnly')
if ($Https) {
    $hc = Get-OrAdd $sw 'httpCookies'
    $hc.SetAttribute('httpOnlyCookies', 'true')
    $hc.SetAttribute('requireSSL', 'true')
}

# --- ELMAH: the repo leaves /elmah open to everyone ---------------------------------
$as = $cfg.SelectSingleNode('appSettings')
$elmah = [ordered]@{
    'elmah.mvc.requiresAuthentication' = 'true'
    'elmah.mvc.allowedRoles'           = 'Super Administrators'
    'elmah.mvc.allowedUsers'           = ''
}
foreach ($k in $elmah.Keys) {
    $nodes = @($as.SelectNodes("add[@key='$k']"))
    for ($i = 1; $i -lt $nodes.Count; $i++) { [void]$as.RemoveChild($nodes[$i]) }   # the repo has duplicates; last one won
    $n = if ($nodes.Count) { $nodes[0] } else { $x = $as.AppendChild($xml.CreateElement('add')); $x.SetAttribute('key', $k); $x }
    $n.SetAttribute('value', $elmah[$k])
}

$xml.Save($path)
Write-Host "Saved $path"

if ($EncryptConnectionStrings) {
    $regiis = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe'
    & $regiis -pef connectionStrings $SitePath -prov DataProtectionConfigurationProvider
    if ($LASTEXITCODE) { throw "aspnet_regiis failed ($LASTEXITCODE)" }
    Write-Host 'connectionStrings encrypted with DPAPI (only this machine can decrypt it).'
}

# --- quick connectivity test using the same strings ----------------------------------
foreach ($name in $wanted.Keys) {
    $conn = New-Object System.Data.SqlClient.SqlConnection (New-ConnString $wanted[$name])
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand(); $cmd.CommandText = 'SELECT DB_NAME()'
        Write-Host ("  OK   {0}: connected to {1}" -f $name, $cmd.ExecuteScalar()) -ForegroundColor Green
    } catch {
        Write-Warning ("  FAIL {0}: {1}" -f $name, $_.Exception.Message)
        if ($IntegratedSecurity) { Write-Warning '  (Windows auth: this test runs as YOU, not as the app pool identity.)' }
    } finally { $conn.Dispose() }
}

Import-Module WebAdministration -ErrorAction SilentlyContinue
if (Get-Command Restart-WebAppPool -ErrorAction SilentlyContinue) {
    if (Test-Path "IIS:\AppPools\$AppPoolName") { Restart-WebAppPool $AppPoolName; Write-Host "Recycled app pool $AppPoolName" }
}
