<#
.SYNOPSIS
  Build and run the MOTMaster/DAQ unit tests. Never launches MOTMaster or touches hardware:
  the tests only construct classes and call pure routing/mapping code.

.DESCRIPTION
  Compiles every MOTMasterTests\*.cs with the Roslyn csc.exe that ships with VS 2019 (C# 7.3),
  referencing MOTMaster.exe and DAQ.dll from MOTMaster\bin\<Configuration>. The output exe is
  written to that same folder so DAQ, MOTMaster and the NI assemblies resolve beside it.
  Build MOTMaster first (MSBuild MOTMaster\MOTMaster.csproj /p:Configuration=CaF).

  Must run under PowerShell, not Git Bash (a leading "/" flag gets mangled by MSYS).

.PARAMETER Configuration
  Build configuration whose bin\ output to reference (default: CaF).

.PARAMETER BinDir
  Override the folder holding MOTMaster.exe / DAQ.dll and receiving MOTMasterTests.exe. Use it when a
  running MOTMaster locks MOTMasterin\<Configuration> (build with MSBuild /p:OutDir=<dir>).

.EXAMPLE
  powershell -ExecutionPolicy Bypass -File MOTMasterTests\run-tests.ps1
#>
param([string]$Configuration = 'CaF', [string]$BinDir)

$ErrorActionPreference = 'Stop'
$testDir = $PSScriptRoot
$repoRoot = Split-Path $testDir -Parent
$bin = if ($BinDir) { $BinDir } else { Join-Path $repoRoot "MOTMaster\bin\$Configuration" }
$csc = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe'
$out = Join-Path $bin 'MOTMasterTests.exe'

foreach ($f in @($csc, (Join-Path $bin 'MOTMaster.exe'), (Join-Path $bin 'DAQ.dll'))) {
    if (-not (Test-Path $f)) { Write-Error "Missing $f (build MOTMaster with /p:Configuration=$Configuration first)"; exit 2 }
}

$sources = Get-ChildItem -Path $testDir -Filter *.cs | ForEach-Object { $_.FullName }
& $csc /nologo /target:exe /langversion:7.3 "/out:$out" `
    "/reference:$(Join-Path $bin 'MOTMaster.exe')" "/reference:$(Join-Path $bin 'DAQ.dll')" `
    "/reference:$(Join-Path $bin 'NationalInstruments.DAQmx.dll')" $sources
if ($LASTEXITCODE -ne 0) { Write-Host 'Test harness failed to compile.'; exit 2 }

& $out $repoRoot
exit $LASTEXITCODE
