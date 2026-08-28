<#
.SYNOPSIS
  Check that MOTMaster pattern scripts will actually run, without launching MOTMaster or
  touching any hardware: compiles each script, instantiates it, and builds its digital/
  analog pattern - exactly what Controller does before it ever configures a board.

.DESCRIPTION
  Needs only csc.exe - no .NET SDK, no VS Code.

  MOTMaster never prebuilds MoleculeMOTMasterScripts.csproj (that project has no CaF
  build configuration and is IDE-only). Every script is compiled on the fly when it is
  selected in the GUI, then instantiated, then GetSequence()'d and built. A script can
  compile cleanly and still fail at either of those later steps - e.g. a typo'd channel
  name (a NullReferenceException two frames downstream - see the "Hashtable lookups
  return null" gotcha in references/architecture.md), a pattern that doesn't fit in
  PatternLength (InsufficientPatternLengthException), or a bad globalParameters.txt row
  that throws when Convert.ChangeType'd. This builds ScriptSmokeTest.cs together with
  ScriptCompileCheck.cs (both in this same folder - the latter is the actual check logic)
  and runs the result,
  which reproduces Controller.compileFromFile + loadScriptFromDLL + GetSequence() +
  buildPattern for every top-level script, and reports a pass/fail per file. It never
  calls initializeHardware/run/releaseHardware and never touches a DAQMxPatternGenerator
  or the DDS card - the same safe boundary Controller.ViewPattern() (the "View Pattern"
  button) already stops at.

  Must run under PowerShell, not Git Bash: a leading "/" flag like /nologo gets mangled
  into a bogus MSYS path when csc.exe is invoked through the Bash tool.

.PARAMETER Configuration
  Build configuration whose bin\ output to reference (default: CaF, the cafmot config).

.PARAMETER ScriptFolder
  Folder of top-level *.cs scripts to check (default: MoleculeMOTMasterScripts).
  Subfolders (OLD, OldDDS, preJuly26, etc.) are never part of the real script list and
  are not scanned.

.EXAMPLE
  .\verify-scripts.ps1
  .\verify-scripts.ps1 -ScriptFolder AlFMOTMasterScripts -Configuration AlF
#>
param(
    [string]$Configuration = "CaF",
    [string]$ScriptFolder = "MoleculeMOTMasterScripts"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))

$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if (-not (Test-Path $csc)) {
    throw "csc.exe not found at $csc - update this path for the .NET Framework version installed here."
}

$motMasterExe = Join-Path $repoRoot "MOTMaster\bin\$Configuration\MOTMaster.exe"
$daqDll       = Join-Path $repoRoot "DAQ\bin\$Configuration\DAQ.dll"
foreach ($f in @($motMasterExe, $daqDll)) {
    if (-not (Test-Path $f)) {
        throw "$f does not exist. Build MOTMaster.csproj under the $Configuration configuration first (see SKILL.md's Verify section)."
    }
}

$references = @($motMasterExe, $daqDll)

# Mirrors DAQ's per-machine FileSystem.Paths["AdditionalMOTMasterAssemblies"] - currently
# just the DDS driver, only present for configurations that build it (CaF).
$spectrumDds = Join-Path $repoRoot "SpectrumDDS\bin\$Configuration\SpectrumDDS.dll"
if (Test-Path $spectrumDds) { $references += $spectrumDds }

$scriptDir = if ([System.IO.Path]::IsPathRooted($ScriptFolder)) { $ScriptFolder } else { Join-Path $repoRoot $ScriptFolder }
if (-not (Test-Path $scriptDir)) {
    throw "$scriptDir does not exist."
}

# ---- Build the smoke-test harness itself, referencing the same assemblies scripts do ----
$harnessSource = Join-Path $PSScriptRoot "ScriptSmokeTest.cs"
$sharedLogic   = Join-Path $PSScriptRoot "ScriptCompileCheck.cs"
if (-not (Test-Path $sharedLogic)) {
    throw "$sharedLogic does not exist - ScriptSmokeTest.cs depends on it (the check logic)."
}
$harnessExe    = Join-Path $env:TEMP "motmaster_script_smoke_test.exe"
$refArgs       = $references | ForEach-Object { "/reference:`"$_`"" }

$buildArgs = @("/nologo", "/target:exe", "/out:`"$harnessExe`"") + $refArgs + @("`"$harnessSource`"", "`"$sharedLogic`"")
$buildOutput = & $csc $buildArgs 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build ScriptSmokeTest.cs itself:" -ForegroundColor Red
    Write-Host ($buildOutput -join "`n")
    exit 1
}

# ---- Run it: one process checks every top-level script in $scriptDir ----
& $harnessExe $scriptDir @references
exit $LASTEXITCODE
