<#
.SYNOPSIS
  Builds WavemeterLock.exe and "Wavemeter Lock Server.exe" (and their DAQ/SharedCode
  dependencies) via the EDMSuite solution's "Wavemeter" configuration.

.DESCRIPTION
  Neither WavemeterLock.csproj nor WavemeterLockServer.csproj can be built directly with
  a plain Configuration like "Debug" or "Release" — their project references (DAQ.csproj,
  SharedCode.csproj) don't define that configuration, only experiment-named ones. The
  EDMSuite.sln solution defines a "Wavemeter" solution configuration that maps each
  project to a configuration it actually has (see EDMSuite.sln's
  ProjectConfigurationPlatforms section, entries for the WavemeterLock/WavemeterLockServer
  project GUIDs). Building through the .sln with /p:Configuration=Wavemeter resolves this
  automatically. This script does exactly that, restricted to just these two targets so it
  doesn't build the other ~25 unrelated experiment projects in the solution.

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File .claude\skills\wavemeter-lock\build.ps1
#>

$ErrorActionPreference = "Stop"

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..\..")).Path
$sln = Join-Path $repoRoot "EDMSuite.sln"

$vswhere = "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = $null
if (Test-Path $vswhere) {
    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\amd64\MSBuild.exe' 2>$null | Select-Object -First 1
}
if (-not $msbuild -or -not (Test-Path $msbuild)) {
    # Fallback: known-good path on this repo's usual build machine (PH-BONESAW, VS2019 Enterprise).
    $msbuild = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\amd64\MSBuild.exe"
}
if (-not (Test-Path $msbuild)) {
    throw "Could not find MSBuild.exe. Install Visual Studio 2019+ with the .NET desktop workload, or edit the fallback path in this script."
}

Write-Host "Using MSBuild: $msbuild"
Write-Host "Building WavemeterLock + WavemeterLockServer (Configuration=Wavemeter, Platform=Any CPU)..."

& $msbuild $sln /t:"WavemeterLock;WavemeterLockServer" /p:Configuration=Wavemeter /p:Platform="Any CPU" /nologo /v:minimal
$exitCode = $LASTEXITCODE

if ($exitCode -eq 0) {
    Write-Host ""
    Write-Host "BUILD SUCCEEDED"
    Write-Host "  Client: $repoRoot\WavemeterLock\bin\Release\WavemeterLock.exe"
    Write-Host "  Server: $repoRoot\WaveMeterExample\bin\Release\Wavemeter Lock Server.exe"
} else {
    Write-Host ""
    Write-Host "BUILD FAILED (exit code $exitCode)"
}

exit $exitCode
