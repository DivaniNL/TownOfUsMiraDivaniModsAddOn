#Requires -Version 5.1

<#
.SYNOPSIS
    Builds DivaniMods in Release and packages it for a GitHub release.

.DESCRIPTION
    Produces two artifacts in the ./release/ folder:

      - DivaniMods.dll                   - the bare plugin DLL (drag into
                                           Among Us/BepInEx/plugins/)
      - DivaniMods-v<version>.zip        - drop-in package; extracting it into
                                           the Among Us folder places the DLL
                                           under BepInEx/plugins/

    Mirrors what Town-Of-Us-R publishes: one DLL + one zip per release.

.PARAMETER Version
    Version string (without the 'v' prefix), e.g. 1.0.0

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.EXAMPLE
    ./scripts/Package.ps1 -Version 1.0.0
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

Write-Host "==> Building DivaniMods $Version ($Configuration)" -ForegroundColor Cyan
dotnet build DivaniMods.csproj -c $Configuration /nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$dll = Join-Path $repoRoot "bin/$Configuration/net6.0/DivaniMods.dll"
if (-not (Test-Path $dll)) { throw "Expected build output not found: $dll" }

$releaseDir = Join-Path $repoRoot "release"
if (Test-Path $releaseDir) { Remove-Item $releaseDir -Recurse -Force }
New-Item -ItemType Directory -Path $releaseDir | Out-Null

$stage = Join-Path $releaseDir "stage"
$pluginDir = Join-Path $stage "BepInEx/plugins"
New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null

Copy-Item $dll (Join-Path $pluginDir "DivaniMods.dll") -Force
Copy-Item $dll (Join-Path $releaseDir "DivaniMods.dll") -Force

$readmePath = Join-Path $repoRoot "README.md"
if (Test-Path $readmePath) { Copy-Item $readmePath (Join-Path $stage "README.md") -Force }

$zipPath = Join-Path $releaseDir ("DivaniMods-v{0}.zip" -f $Version)
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zipPath -Force

Remove-Item $stage -Recurse -Force

Write-Host ""
Write-Host "==> Release artifacts ready:" -ForegroundColor Green
Write-Host ("    {0}" -f (Join-Path $releaseDir "DivaniMods.dll"))
Write-Host ("    {0}" -f $zipPath)
