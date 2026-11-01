---
name: motmaster-python-integration
description: Driving MOTMaster from Python through pythonnet / .NET Remoting (pycaf, pytweezer, the UEDM/LatticeEDM scripts) — connecting to the Controller, SetScriptPath/Go(dict) scan loops, and which MOTMaster failures a Python caller does and doesn't see.
---

# MOTMaster Python integration

**This skill is a stub and needs filling out.** So far it only records the one fact
below. Add to it (asking the user first) as Python-side work turns up more.

For MOTMaster's own C# internals, see the `motmaster-controller` skill.

## Errors that never reach Python

`Controller.Go(dict)` logs and rethrows most exceptions, so a remote caller sees them.
But some failures **end in a MessageBox on the MOTMaster PC, and then `Go` returns
normally**, so to Python the run looks like it succeeded:

- the script fails to compile (`prepareScript` returns null → "Unable to load pattern");
- either `InsufficientPatternLengthException` (digital or analog), which `goClaimed`
  catches and reports in a MessageBox.

A Python scan loop can't tell these shots from good ones by the return value of `Go`.

**Linked runs are the exception:** while the primary is linked (below), every failure —
compile, pattern length, DDS arm, fingerprint/clock mismatch, secondary not arming or
never seeing the trigger — is raised from `Go` as a `LinkedRunException`.

## Linked mode (one script across two MOTMasters)

On a primary whose hardware declares a link (cafmot/PH-BONESAW → RbTweezer), call
`motmaster.SetLinked(True)` once on the primary's `Controller` proxy; existing
`SetScriptPath` / `Go(dict)` scan loops then run linked unchanged, and each `Go`'s
parameters (including `SecondaryTriggerDelay`) are sent to the secondary. `SetLinked(False)`
releases it; `IsLinked` reports the state. While linked, the **secondary** refuses its own
`Go`/`Run`/`SetScriptPath`/`SetTriggered` — drive scans through the primary. Design and
details: `docs/superpowers/specs/2026-10-22-linked-motmaster-design.md`.
