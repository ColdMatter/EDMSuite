# Bench work: driving the card from Python and PowerShell

Read this before running anything that opens the card. The environment has traps
that cost hours the first time round.

## Environment traps

**The Bash tool is sandboxed and silently kills any process that opens the
Spectrum device.** `spcm_hOpen` produces exit code 0, no output and no traceback —
it looks like your script did nothing. **Use the PowerShell tool for anything that
touches the card.** Bash is still fine for `git`, `grep` and reading files.

**Do not feed `python -c` from a PowerShell here-string** (`@'...'@`). The program
arrives empty and python exits 0 silently. Multi-line strings in the PowerShell
tool have also triggered spurious hook errors. Write a `.py` file and run it —
it is also easier to iterate on and leaves a record.

**PowerShell 7 cannot compile C# from source.** `CSharpCodeProvider` throws
"Operation is not supported on this platform" because pwsh is .NET Core. Use
Windows PowerShell for that: `& "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File <script>`.

**`Add-Type -Path` refuses `.exe`.** To load an executable assembly use
`[System.Reflection.Assembly]::LoadFrom(...)` and `GetType(...)`.

**PowerShell cannot drive a .NET remoting proxy.** Its object adapter throws inside
`InternalDeserializer` on a property setter. Test remoting from C#.

**Loading assemblies out of a scratch build directory** needs an
`AppDomain.AssemblyResolve` handler probing that directory, guarded against
re-entry — `LoadFrom` re-enters it per dependency and will otherwise recurse to a
stack overflow.

## Python environment

```powershell
Set-Location C:\ControlPrograms\EDMSuite\dds_python
poetry run python -u probe.py
```

The venv already has `spcm ^1.13.3`, `nidaqmx`, numpy, scipy, matplotlib.

The imports are `spcm_core.*`, **not** `spcm.*` — `spcm.pyspcm` and
`spcm.constants` do not exist. The low-level API is worth preferring over the
high-level `spcm` package because it mirrors the C API the C# driver uses, so what
you prove here transfers one-to-one:

```python
from spcm_core.pyspcm import (
    spcm_hOpen, spcm_vClose,
    spcm_dwSetParam_i32, spcm_dwSetParam_i64, spcm_dwSetParam_d64,
    spcm_dwGetParam_i32, spcm_dwGetParam_i64, spcm_dwGetParam_d64,
    spcm_dwGetErrorInfo_i32,
)
from spcm_core.regs import *   # SPC_* and SPCM_DDS_* constants
```

`dds_python/spectrum_dds.py` wraps this as a `DDSCard` context manager with checked
register access, routing with read-back, priming and core helpers. Build on it
rather than reopening the device by hand.

## What the existing scripts are for

| Script | Purpose |
|---|---|
| `probe.py` | Identity, feature bits, capability registers, routing incl. the snapping demo, queue state. Good regression check. `timer_range.py` bisects the timer range separately. |
| `timing_model.py` | Settles the block-boundary question. Prints "HYPOTHESIS A holds". |
| `run_pattern.py` | Compiles a realistic 6-event pattern and runs N shots with `FORCETRIGGER`, checking trigger counts and underrun/overrun. `--outputs` enables RF for a scope check. |
| `debug_*.py` | Narrow diagnostics, each pinning down one erratum. Named after what they investigate. |

Adding a `debug_<thing>.py` for a new question is the normal move — it keeps the
evidence and makes the finding reproducible.

## Reading register constants and probing capability

Get register numbers and mode values from the installed driver (`import
spcm_core.regs as R`), never from the PDF. To find out what the card actually
supports, read its availability bitmap and try the candidates — this is how
erratum 9 was settled:

```python
print(hex(get(R.SPCM_X0_AVAILMODES)))
for name in (n for n in dir(R) if n.startswith("SPCM_XMODE_")):
    rc = spcm_dwSetParam_i32(h, R.SPCM_X0_MODE, getattr(R, name))
    print(name, "ACCEPTED" if rc == 0 else "rejected")
```

Always restore what you changed before closing.

## Checking the card is free

The GUI, MOTMaster and a bench script all want the same single handle. If open
fails with "could not open /dev/spcm0", something else holds it:

```powershell
Get-Process | Where-Object { $_.ProcessName -match "SpectrumDDS|MOTMaster|MoleculeMOT" } |
    Select-Object Id, ProcessName, MainWindowTitle
```

**Do not kill these** — they are the experimenter's control programs. Ask, or work
on something else until the card is free.

## Testing safely

Use `M2CMD_CARD_FORCETRIGGER` as the stand-in for the pulse on `DDS_Analog_Trg` —
that line also starts the AO boards, so driving it for real is the user's call.
Enabling an output stage with all cores silenced emits nothing, which is what makes
routing and status tests safe to run unsupervised.

End a run the way `SpectrumDDSDriver.Close()` does: silence all cores, `EXEC_NOW`,
`WRITE_TO_CARD`, stop the card. Then the next person does not inherit a card
mid-pattern.
