<#
.SYNOPSIS
  Builds C8.eServices.Mvc in Release and produces a deployable zip.

.DESCRIPTION
  Run on a Windows build machine (not the web server) that has:
    - Visual Studio 2019/2022 or "Build Tools for Visual Studio" with the
      "ASP.NET and web development build tools" workload
    - .NET Framework 4.6.1 targeting pack (4.8 developer pack also works)
    - git on PATH (only for stamping VERSION.txt)
  nuget.exe is downloaded next to this script if it is not on PATH.

  Steps: NuGet restore -> MSBuild Release file-system publish -> sanity checks
  (views, PDF templates, wkhtmltopdf, no debug compilation) -> zip with VERSION.txt.

  The zip contains the repo's Web.config as-is. Do NOT edit it in the zip: the
  server-side script IIS\Set-WebConfig.ps1 writes the real connection strings.

.EXAMPLE
  .\Build-Package.ps1
  .\Build-Package.ps1 -OutputDir D:\Releases
#>
[CmdletBinding()]
param(
    [string]$Configuration = 'Release',
    [string]$OutputDir = (Join-Path $PSScriptRoot '..\out')
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repo    = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$proj    = Join-Path $repo 'C8.eServices.Mvc\C8.eServices.Mvc.csproj'
$sln     = Join-Path $repo 'C8.eServices.sln'
$OutputDir = [IO.Path]::GetFullPath($OutputDir)
$publish = Join-Path $OutputDir 'publish'

function Step($m) { Write-Host "`n==> $m" -ForegroundColor Cyan }

# --- 1. Tools -------------------------------------------------------------
Step 'Locating MSBuild'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path $vswhere)) { throw "vswhere.exe not found. Install Visual Studio or Build Tools for Visual Studio." }
$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'MSBuild not found via vswhere.' }
Write-Host $msbuild

$webTargets = Get-ChildItem (Split-Path (Split-Path (Split-Path $msbuild))) -Recurse -Filter 'Microsoft.WebApplication.targets' -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $webTargets) { throw 'Microsoft.WebApplication.targets missing: add the "ASP.NET and web development build tools" workload.' }

$nuget = (Get-Command nuget.exe -ErrorAction SilentlyContinue).Source
if (-not $nuget) {
    $nuget = Join-Path $PSScriptRoot 'nuget.exe'
    if (-not (Test-Path $nuget)) {
        Step 'Downloading nuget.exe'
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
        Invoke-WebRequest 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile $nuget
    }
}

# --- 2. Restore + publish ---------------------------------------------------
Step 'Restoring NuGet packages'
& $nuget restore $sln -NonInteractive
if ($LASTEXITCODE) { throw "nuget restore failed ($LASTEXITCODE)" }

Step "Publishing ($Configuration) to $publish"
if (Test-Path $publish) { Remove-Item $publish -Recurse -Force }
New-Item -ItemType Directory -Force -Path $publish | Out-Null
& $msbuild $proj /nologo /m /v:minimal `
    /p:Configuration=$Configuration /p:Platform=AnyCPU `
    /p:DeployOnBuild=true /p:WebPublishMethod=FileSystem /p:PublishProvider=FileSystem `
    /p:publishUrl="$publish" /p:DeleteExistingFiles=true /p:ExcludeApp_Data=false
if ($LASTEXITCODE) { throw "MSBuild failed ($LASTEXITCODE)" }

# --- 3. Clean-up: things that must not ship ----------------------------------
Step 'Removing dev-only files from the package'
$drop = @('Tests', 'obj', 'Properties\PublishProfiles')
foreach ($d in $drop) { $p = Join-Path $publish $d; if (Test-Path $p) { Remove-Item $p -Recurse -Force } }
Get-ChildItem $publish -Include *.md, *.pdb, *.user, *.sql -Recurse -File | Where-Object { $_.FullName -notlike '*\Scripts\*' } | Remove-Item -Force
# Generated documents (lease agreements, debit orders, uploads) hold personal data and
# belong to the live server, not the package. Keep only the templates the code opens.
foreach ($d in 'PDFFiles', 'Uploads', 'UploadedFiles', 'Section49PDFs', 'Logs', 'Results') {
    $p = Join-Path $publish $d
    if (Test-Path $p) { Get-ChildItem $p -Recurse -File | Remove-Item -Force }
}
$tpl = Join-Path $publish 'Templates'
if (Test-Path $tpl) {
    Get-ChildItem $tpl -File | Where-Object { $_.Name -notin @('PLMEvictionNotice.pdf', 'pdf_viewer.html') } | Remove-Item -Force
}

# --- 4. Sanity checks ---------------------------------------------------------
Step 'Checking the package'
$problems = @()
[xml]$cs = Get-Content $proj
$ns = @{ m = 'http://schemas.microsoft.com/developer/msbuild/2003' }
$content = Select-Xml -Xml $cs -XPath '//m:Content/@Include' -Namespace $ns | ForEach-Object { $_.Node.Value }
foreach ($c in $content | Where-Object { $_ -like 'Views\*.cshtml' -or $_ -like 'PDFTemplates\*' }) {
    if (-not (Test-Path -LiteralPath (Join-Path $publish $c))) { $problems += "missing from package: $c" }
}
# Views on disk that are not in the project are silently dropped by Publish.
$srcViews = Get-ChildItem (Join-Path $repo 'C8.eServices.Mvc\Views') -Recurse -Filter *.cshtml |
    ForEach-Object { $_.FullName.Substring((Join-Path $repo 'C8.eServices.Mvc').Length + 1) }
$notInProj = $srcViews | Where-Object { $_ -notin $content }
if ($notInProj) { Write-Warning ("{0} view(s) on disk are not in the .csproj and were NOT packaged:`n  {1}" -f @($notInProj).Count, ($notInProj -join "`n  ")) }
if (-not (Test-Path (Join-Path $publish 'Rotativa\wkhtmltopdf.exe'))) { $problems += 'Rotativa\wkhtmltopdf.exe missing (PDF generation will fail)' }
if (-not (Test-Path (Join-Path $publish 'bin\C8.eServices.Mvc.dll'))) { $problems += 'bin\C8.eServices.Mvc.dll missing' }
[xml]$wc = Get-Content (Join-Path $publish 'Web.config')
if ($wc.configuration.'system.web'.compilation.debug -eq 'true') { $problems += 'Web.config still has compilation debug="true" (Release transform not applied?)' }
if ($problems) { $problems | ForEach-Object { Write-Error $_ -ErrorAction Continue }; throw 'Package checks failed.' }

# --- 5. Version stamp + zip -----------------------------------------------------
Step 'Writing VERSION.txt and zipping'
$branch = (git -C $repo rev-parse --abbrev-ref HEAD) 2>$null
$commit = (git -C $repo rev-parse --short HEAD) 2>$null
$dirty  = if ((git -C $repo status --porcelain) 2>$null) { ' (uncommitted changes present)' } else { '' }
$stamp  = Get-Date -Format 'yyyyMMdd-HHmm'
@"
PLM-EHC deployment package
Built      : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz') on $env:COMPUTERNAME by $env:USERNAME
Branch     : $branch
Commit     : $commit$dirty
Config     : $Configuration
"@ | Set-Content (Join-Path $publish 'VERSION.txt') -Encoding UTF8

$zip = Join-Path $OutputDir ("PLM-EHC_{0}_{1}_{2}.zip" -f ($branch -replace '[\\/]', '-'), $commit, $stamp)
if (Test-Path $zip) { Remove-Item $zip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::CreateFromDirectory($publish, $zip, [IO.Compression.CompressionLevel]::Optimal, $false)

# Bundle the deployment scripts alongside, so one folder goes to the server.
$kit = Join-Path $OutputDir 'Deployment'
if (Test-Path $kit) { Remove-Item $kit -Recurse -Force }
New-Item -ItemType Directory -Path $kit | Out-Null
Copy-Item (Join-Path $repo 'Deployment\README.md'), (Join-Path $repo 'Deployment\Database'), (Join-Path $repo 'Deployment\IIS') -Destination $kit -Recurse

Write-Host "`nPackage : $zip" -ForegroundColor Green
Write-Host "Scripts : $kit" -ForegroundColor Green
Write-Host 'Copy both to the server and follow Deployment\README.md.'
